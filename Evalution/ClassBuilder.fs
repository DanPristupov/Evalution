namespace Evalution
open System
open System.Reflection
open System.Reflection.Emit
open Sigil
open Sigil.NonGeneric

type PropertyExpression = {Property : PropertyInfo; Expr: string }

type public ClassBuilder(targetType:Type) =

    let propertyExpressions = new ResizeArray<PropertyExpression>()
    let typeProperties = lazy (targetType.GetProperties())

    let createType (objType: Type):Type =

        let createTypeBuilder (objType:Type) =
            // todo: should assemblyName be the same for all classes?
            let assemblyName = new AssemblyName("EV_" + objType.Name) // may be I should use assembly of the objType?
            let assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run)
            let moduleBuilder = assemblyBuilder.DefineDynamicModule("EvalutionModule");
            let typeBuilder = moduleBuilder.DefineType("EV" + objType.Name,
                TypeAttributes.Public ||| TypeAttributes.Class ||| TypeAttributes.AutoClass |||
                TypeAttributes.AnsiClass ||| TypeAttributes.BeforeFieldInit ||| TypeAttributes.AutoLayout,
                null)
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public ||| MethodAttributes.SpecialName |||
                                                    MethodAttributes.RTSpecialName) |> ignore
            typeBuilder.SetParent(objType)
            typeBuilder

        let typeBuilder = createTypeBuilder objType
        let targetTypeProperties = objType.GetProperties()

        let createGetPropertyMethodBuilder(propertyName, propertyType:Type, expression):MethodBuilder =
            let emitter = Emit.BuildMethod(propertyType, Array.empty, typeBuilder, "get_"+propertyName,
                MethodAttributes.Public ||| MethodAttributes.SpecialName ||| MethodAttributes.HideBySig |||
                MethodAttributes.Virtual,
                CallingConventions.Standard ||| CallingConventions.HasThis)
            
            let rec generateMulticallBody (multicall: Ast.Multicall, thisType: Type) =
                let createPropertyCall (typeProperties : PropertyInfo[], propertyName) =
                    let targetProperty = typeProperties |> Seq.find(fun x -> x.Name = propertyName)
                    let getMethodPropertyInfo = targetProperty.GetGetMethod()
                    emitter.CallVirtual(getMethodPropertyInfo) |> ignore
                    targetProperty.PropertyType

                match multicall with
                | Ast.ThisPropertyCall (identifier) ->
                    let (Ast.Identifier targetPropertyName) = identifier
                    emitter.LoadArgument(uint16 0) |> ignore    // Emit: load this reference onto stack
                    createPropertyCall(targetTypeProperties, targetPropertyName)
                | Ast.ObjectPropertyCall (prevCall, ident) ->
                    let subPropertyType = generateMulticallBody(prevCall, thisType)
                    let (Ast.Identifier targetPropertyName) = ident
                    createPropertyCall(subPropertyType.GetProperties(), targetPropertyName)

            let rec generateMethodBody (program: Ast.Program) =
                match program with
                | Ast.BinaryExpression (leftExpr, operator, rightExpr) ->
                    // todo: need to handle different types of expression here.
                    // need to call op_Addition to add TimeSpans...
                    // probably need to create a matrix of allowed math operations...
                    let loadExpressionResultOnStack () =
                        generateMethodBody leftExpr
                        generateMethodBody rightExpr

                    match operator with
                    | Ast.Add ->
                        loadExpressionResultOnStack()
                        emitter.Add() |> ignore
                    | Ast.Subtract ->
                        loadExpressionResultOnStack()
                        emitter.Subtract() |> ignore
                    | Ast.Multiply ->
                        loadExpressionResultOnStack()
                        emitter.Multiply() |> ignore
                    | Ast.Divide ->
                        loadExpressionResultOnStack()
                        emitter.Divide() |> ignore
                | Ast.LiteralExpression (literal) ->
                    match literal with
                    | Ast.Int32Literal (v) ->
                        emitter.LoadConstant(v) |> ignore
                    | Ast.DoubleLiteral (v) ->
                        emitter.LoadConstant(v) |> ignore
                    | Ast.TimeSpanLiteral (v) ->
                        let fromTicksMethod = typeof<TimeSpan>.GetMethod("FromTicks")
                        emitter.LoadConstant(v.Ticks) |> ignore
                        emitter.Call(fromTicksMethod) |> ignore
                    | _ -> failwith "Unknown literal"
                | Ast.MultiCallExpression (multicall) ->
                    generateMulticallBody(multicall, objType) |> ignore

            generateMethodBody(AstBuilder.build expression)
            emitter.Return() |> ignore
            emitter.CreateMethod()

        let buildProperty (property:PropertyInfo, expression) =
            let propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.PropertyType, null);
                
            let getMethodBuilder = createGetPropertyMethodBuilder(property.Name, property.PropertyType, expression)
            propertyBuilder.SetGetMethod(getMethodBuilder) |> ignore

        for propertyExpr in propertyExpressions do
            buildProperty(propertyExpr.Property, propertyExpr.Expr)

        typeBuilder.CreateType()


    member this.Setup (property: string, expression: string) :ClassBuilder =
        let propertyInfo = typeProperties.Force() |> Array.find (fun x -> x.Name = property)
        propertyExpressions.Add({ Property= propertyInfo; Expr = expression } )
        this

    member this.BuildObject ():obj =
        let resultType = createType targetType
        Activator.CreateInstance(resultType)

type public ClassBuilder<'T when 'T: null>() =
    inherit ClassBuilder(typeof<'T>)

    member this.Setup<'TProperty> (property:System.Linq.Expressions.Expression<Func<'T, 'TProperty>>, expression: string):ClassBuilder<'T> = 
        let body = property.Body :?> System.Linq.Expressions.MemberExpression
        base.Setup(body.Member.Name, expression) :?> ClassBuilder<'T>

    member this.BuildObject ():'T = 
        base.BuildObject() :?> 'T

