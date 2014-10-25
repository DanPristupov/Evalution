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

            let createProxyConstructors ()= 
                let createProxyConstructor (ctor:ConstructorInfo) =
                    let params = ctor.GetParameters()
                    let paramTypes = params |> Seq.map (fun x -> x.ParameterType) |> Seq.toArray
                    let emit = Emit.BuildConstructor(paramTypes, typeBuilder, MethodAttributes.Public, CallingConventions.HasThis)
                    emit.LoadArgument(uint16 0) |> ignore
                    let mutable i = 1
                    for param in params do
                        emit.LoadArgument(uint16 i) |> ignore
                        i <- i + 1
                        ()
                    emit.Call(ctor) |> ignore
                    emit.Return()
                    emit.CreateConstructor()
                    ()
                for ctor in objType.GetConstructors() do
                    createProxyConstructor ctor
                ()

            typeBuilder.SetParent(objType)
            createProxyConstructors()

            typeBuilder

        let typeBuilder = createTypeBuilder objType
        let targetTypeProperties = objType.GetProperties()

        let createGetPropertyMethodBuilder(propertyName, propertyType:Type, expression):MethodBuilder =
            let emitter = Emit.BuildMethod(propertyType, Array.empty, typeBuilder, "get_"+propertyName,
                MethodAttributes.Public ||| MethodAttributes.SpecialName ||| MethodAttributes.HideBySig |||
                MethodAttributes.Virtual,
                CallingConventions.Standard ||| CallingConventions.HasThis)
            
            let rec getExpressionType expression objType=
                let rec getMultiCallExpressionType(multicallExpression, objType: Type) =
                    let getPropertyType (typeProperties : PropertyInfo[], propertyName) =
                        let targetProperty = typeProperties |> Seq.find(fun x -> x.Name = propertyName)
                        targetProperty.PropertyType
                    match multicallExpression with
                    | Ast.ThisPropertyCall (identifier) ->
                        let (Ast.Identifier targetPropertyName) = identifier
                        getPropertyType(targetTypeProperties, targetPropertyName)
                    | Ast.ObjectPropertyCall (prevCall, ident) ->
                        let subPropertyType = getMultiCallExpressionType(prevCall, objType)
                        let (Ast.Identifier targetPropertyName) = ident
                        getPropertyType(subPropertyType.GetProperties(), targetPropertyName)
                    | Ast.ArrayElementCall (prevCall, _) ->
                        let subPropertyType = getMultiCallExpressionType(prevCall, objType)
                        subPropertyType.GetElementType()

                match expression with
                | Ast.BinaryExpression(el,_, er) -> getExpressionType el objType
                | Ast.LiteralExpression(literalExpr) ->
                    match literalExpr with
                    | Ast.BoolLiteral(_) -> typeof<bool>
                    | Ast.Int32Literal(_) -> typeof<int>
                    | Ast.DoubleLiteral(_) -> typeof<float>
                    | Ast.TimeSpanLiteral(_) -> typeof<TimeSpan>
                | Ast.UnaryExpression(_,expr) -> getExpressionType expr objType
                | Ast.MultiCallExpression(multicallExpr) -> getMultiCallExpressionType(multicallExpr, objType)

            let rec generateMethodBody (program: Ast.Program) =
                let rec generateMulticallBody (multicall: Ast.Multicall, thisType: Type) =
                    let createPropertyCall (typeProperties : PropertyInfo[], propertyName) =
                        let targetProperty = typeProperties |> Seq.find(fun x -> x.Name = propertyName) // todo: findOrEmpty. Throw an exception that property 'XX' cannot be found in the class 'YY"
                        let getMethodPropertyInfo = targetProperty.GetGetMethod()
                        emitter.CallVirtual(getMethodPropertyInfo) |> ignore
                        targetProperty.PropertyType

                    match multicall with
                    | Ast.ThisPropertyCall (identifier) ->
                        let (Ast.Identifier targetPropertyName) = identifier
                        emitter.LoadArgument(uint16 0) |> ignore    // Emit: load 'this' reference onto stack
                        createPropertyCall(targetTypeProperties, targetPropertyName)
                    | Ast.ObjectPropertyCall (prevCall, ident) ->
                        let subPropertyType = generateMulticallBody(prevCall, thisType)
                        let (Ast.Identifier targetPropertyName) = ident
                        createPropertyCall(subPropertyType.GetProperties(), targetPropertyName)
                    | Ast.ArrayElementCall (prevCall, expr) ->
                        let subPropertyType = generateMulticallBody(prevCall, thisType)
                        generateMethodBody expr
                        let elementType = subPropertyType.GetElementType()
                        emitter.LoadElement(elementType) |> ignore
                        elementType

                match program with
                | Ast.BinaryExpression (leftExpr, operator, rightExpr) ->
                    let leftType = getExpressionType leftExpr objType
                    let rightType = getExpressionType rightExpr objType
                    // todo: need to handle different types of expression here.
                    // need to call op_Addition to add TimeSpans...
                    // probably need to create a matrix of allowed math operations...
                    let loadExpressionResultOnStack () =
                        generateMethodBody leftExpr
                        generateMethodBody rightExpr

                    let isPrimitiveType t =
                        match t with
                        | (x) when x = typeof<int> or x = typeof<float> -> true
                        | _ -> false 

                    match operator with
                    | Ast.Add ->
                        loadExpressionResultOnStack()
                        if isPrimitiveType leftType then
                            emitter.Add() |> ignore
                        else
                            let addMethod = leftType.GetMethod("op_Addition", [|leftType; rightType|])
                            emitter.Call(addMethod) |> ignore
                    | Ast.Subtract ->
                        loadExpressionResultOnStack()
                        if isPrimitiveType leftType then
                            emitter.Subtract() |> ignore
                        else
                            let subtractMethod = leftType.GetMethod("op_Subtraction", [|leftType; rightType|])
                            emitter.Call(subtractMethod) |> ignore
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
                | Ast.UnaryExpression (uExp, expr) ->
                    match uExp with
                    | Ast.LogicalNegate ->
                        failwith "Logical negate is not implemented yet."
                    | Ast.Negate ->
                        generateMethodBody expr
                        emitter.Negate() |> ignore
                    | Ast.Identity ->
                        generateMethodBody expr // We do not need to do anything here.
                | Ast.MultiCallExpression (multicall) ->
                    generateMulticallBody(multicall, objType) |> ignore
                |_ -> failwith "Unknown syntax tree element."

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

    let mutable resultType:Type = null;

    member this.Setup (property: string, expression: string) :ClassBuilder =
        let propertyInfo = typeProperties.Force() |> Array.find (fun x -> x.Name = property)
        propertyExpressions.Add({ Property= propertyInfo; Expr = expression } )
        this

    member this.BuildObject ([<ParamArray>] parameters: Object[]):obj =
        if resultType = null then
            resultType <- createType targetType
        Activator.CreateInstance(resultType, parameters)

type public ClassBuilder<'T when 'T: null>() =
    inherit ClassBuilder(typeof<'T>)

    member this.Setup<'TProperty> (property:System.Linq.Expressions.Expression<Func<'T, 'TProperty>>, expression: string):ClassBuilder<'T> = 
        let body = property.Body :?> System.Linq.Expressions.MemberExpression
        base.Setup(body.Member.Name, expression) :?> ClassBuilder<'T>

    member this.BuildObject ([<ParamArray>] parameters: Object[]):'T = 
        base.BuildObject(parameters) :?> 'T

