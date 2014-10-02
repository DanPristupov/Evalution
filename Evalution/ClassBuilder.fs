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
            let typeSignature = "EV" + objType.Name
            let assemblyName = new AssemblyName(typeSignature) // may be I should use assembly of the objType?
            let assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run)
            let moduleBuilder = assemblyBuilder.DefineDynamicModule("EvalutionModule");
            let typeBuilder = moduleBuilder.DefineType(typeSignature,
                                    TypeAttributes.Public |||
                                    TypeAttributes.Class |||
                                    TypeAttributes.AutoClass |||
                                    TypeAttributes.AnsiClass |||
                                    TypeAttributes.BeforeFieldInit |||
                                    TypeAttributes.AutoLayout,
                                    null)
            typeBuilder

        let typeBuilder = createTypeBuilder objType
        typeBuilder.DefineDefaultConstructor(MethodAttributes.Public |||
                                                MethodAttributes.SpecialName |||
                                                MethodAttributes.RTSpecialName) |> ignore
        typeBuilder.SetParent(objType)

        let createGetPropertyMethodBuilder(propertyName, propertyType:Type, expression, allProperties):MethodBuilder =
            let rec generateMethodBodyInt (emitter:Emit, program: Ast.Program, allProperties: PropertyInfo[]) =
                match program with
                | Ast.BinaryExpression (leftExpr, operator, rightExpression) ->
                    match operator with
                    | Ast.Add ->
                        generateMethodBodyInt(emitter, leftExpr, allProperties)
                        generateMethodBodyInt(emitter, rightExpression, allProperties)
                        emitter.Add() |> ignore
                    | Ast.Multiply ->
                        generateMethodBodyInt(emitter, leftExpr, allProperties)
                        generateMethodBodyInt(emitter, rightExpression, allProperties)
                        emitter.Multiply() |> ignore
                    | _ -> failwith "blah"
                | Ast.LiteralExpression (literal) ->
                    match literal with
                    | Ast.Int32Literal (v) ->
                        emitter.LoadConstant(v) |> ignore
                    | _ -> failwith "blah"
                | Ast.IdentifierExpression (literal) ->
                    match literal with
                    | Ast.Identifier (ident) ->
                        let property = allProperties |> Seq.find(fun x -> x.Name = ident)
                        let getMethod = property.GetGetMethod()
                        emitter.LoadArgument(uint16 0)
                        emitter.CallVirtual(getMethod) |> ignore
                    | _ -> failwith "blah"
                | _ -> failwith "blah"


            let emitter = Emit.BuildMethod(propertyType,Array.empty,typeBuilder, "get_"+propertyName, MethodAttributes.Public ||| MethodAttributes.SpecialName ||| MethodAttributes.HideBySig ||| MethodAttributes.Virtual,CallingConventions.Standard ||| CallingConventions.HasThis)

            generateMethodBodyInt(emitter, (AstBuilder.build expression), allProperties)
            emitter.Return()
            emitter.CreateMethod()

        let createProperty (property:PropertyInfo, expression, allProperties) =
            let propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.PropertyType, null);
                
            let getMethodBuilder = createGetPropertyMethodBuilder(property.Name, property.PropertyType, expression, allProperties)
            propertyBuilder.SetGetMethod(getMethodBuilder);

            ()
        let allProperties = objType.GetProperties()
        for propertyExpr in propertyExpressions do
            createProperty(propertyExpr.Property, propertyExpr.Expr, allProperties)

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
        base.Setup(body.Member.Name, expression)
        this

    member this.BuildObject ():'T = 
        base.BuildObject() :?> 'T

