namespace Evalution
open System
open System.Reflection
open System.Reflection.Emit
open Sigil.NonGeneric

open EvalutionErrors

type PropertyExpression = {Property : PropertyInfo; Expr: string }
// TODO: create a function to receive a list of type properties (with caching)
type public ClassBuilder(targetType:Type) =

    let environmentClasses = new ResizeArray<Type>()
    let propertyExpressions = new ResizeArray<PropertyExpression>()
    let typeProperties = new Collections.Generic.Dictionary<Type, PropertyInfo[]>()

    let objectContexts =
        seq {
            yield targetType
            for environmentClass in environmentClasses do
                yield environmentClass
        }

    let getProperties (t:Type) :PropertyInfo[] =
        if typeProperties.ContainsKey(t) then
            typeProperties.[t]
        else
            let properties = t.GetProperties()
            typeProperties.Add(t, properties)
            properties

    let getCurrentContextProperty (propertyName) = // todo: rename to GetDefaultContextProperty
        let findProperty (t:Type, propertyName) =
            let properties = getProperties t
            match properties |> Seq.tryFind(fun x -> x.Name = propertyName) with
            | Some (property) -> (true, property.GetGetMethod())
            | None -> (false, null)

        // Priorities: CurrentObject, EnvironmentObject
        let result =
            objectContexts
            |> Seq.map(fun x -> (x, findProperty(x, propertyName)))
            |> Seq.tryFind(fun (obj, (success, property)) -> success)

        match result with
        | Some(obj, (success, property)) -> (obj, property)
        | _ -> raise (invalidNameError propertyName)

    let createType (objType: Type):Type =

        let createTypeBuilder (t:Type) =
            // todo: should assemblyName be the same for all classes?
            let assemblyName = new AssemblyName("EV_" + t.Name) // may be I should use assembly of the objType?
            let assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run)
            let moduleBuilder = assemblyBuilder.DefineDynamicModule("EvalutionModule");
            let typeBuilder =
                moduleBuilder.DefineType("EV" + t.Name,
                                        TypeAttributes.Public ||| TypeAttributes.Class ||| TypeAttributes.AutoClass |||
                                        TypeAttributes.AnsiClass ||| TypeAttributes.BeforeFieldInit ||| TypeAttributes.AutoLayout,
                                        null)

            let createProxyConstructors (t: Type)= 
                let createProxyConstructor (ctor:ConstructorInfo) =
                    let parameters = ctor.GetParameters()
                    let paramTypes = parameters |> Seq.map (fun x -> x.ParameterType) |> Seq.toArray
                    let emit = Emit.BuildConstructor(paramTypes, typeBuilder, MethodAttributes.Public, CallingConventions.HasThis)
                    emit.LoadArgument(uint16 0) |> ignore
                    parameters |> Seq.iteri(fun i _ -> emit.LoadArgument(uint16 (i+1)) |> ignore)
                    emit.Call(ctor) |> ignore
                    emit.Return() |> ignore
                    emit.CreateConstructor() |> ignore

                t.GetConstructors() |> Seq.iter(fun x -> createProxyConstructor x)

            typeBuilder.SetParent(t)
            createProxyConstructors(t)

            typeBuilder

        let typeBuilder = createTypeBuilder objType

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
                    | Ast.CurrentContextPropertyCall (identifier) ->
                        let (target, property) = getCurrentContextProperty identifier
                        property.ReturnType
                    | Ast.ObjectContextPropertyCall (prevCall, identifier) ->
                        let subPropertyType = getMultiCallExpressionType(prevCall, objType)
                        getPropertyType(getProperties(subPropertyType), identifier)
                    | Ast.ArrayElementCall (prevCall, _) ->
                        let subPropertyType = getMultiCallExpressionType(prevCall, objType)
                        subPropertyType.GetElementType()
                    | _ -> failwith "Unknown multicall expression"

                match expression with
                | Ast.BinaryExpression(el,_, er) -> getExpressionType el objType
                | Ast.LiteralExpression(literalExpr) ->
                    match literalExpr with
                    | Ast.BoolLiteral(_) -> typeof<bool>
                    | Ast.Int32Literal(_) -> typeof<int>
                    | Ast.DoubleLiteral(_) -> typeof<float>
                | Ast.UnaryExpression(_,expr) -> getExpressionType expr objType
                | Ast.TimeSpanExpression(_) -> typeof<TimeSpan>
                | Ast.MultiCallExpression(multicallExpr) -> getMultiCallExpressionType(multicallExpr, objType)
                | _ -> failwith "Unknown expression"

            let rec generateMethodBody (program: Ast.Program) =
                let rec generateMulticallBody (multicall: Ast.Multicall, thisType: Type) =

                    let createPropertyCall (typeProperties : PropertyInfo[], propertyName) =
                        let targetProperty = typeProperties |> Seq.find(fun x -> x.Name = propertyName) // todo: findOrEmpty. Throw an exception that property 'XX' cannot be found in the class 'YY"
                        let getMethodPropertyInfo = targetProperty.GetGetMethod()
                        emitter.CallVirtual(getMethodPropertyInfo) |> ignore
                        targetProperty.PropertyType

                    let createStaticPropertyCall (propertyMethod : MethodInfo) =
                        emitter.Call(propertyMethod) |> ignore
                        propertyMethod.ReturnType

                    match multicall with
                    | Ast.CurrentContextPropertyCall (identifier) ->
                        let (target, property) = getCurrentContextProperty identifier
                        if target = thisType then
                            emitter.LoadArgument(uint16 0) |> ignore    // Emit: load 'this' reference onto stack
                            createPropertyCall(getProperties(targetType), identifier)
                        else
                            createStaticPropertyCall(property)

                    | Ast.ObjectContextPropertyCall (prevCall, identifier) ->
                        let subPropertyType = generateMulticallBody(prevCall, thisType)
                        createPropertyCall(getProperties(subPropertyType), identifier)
                    | Ast.ArrayElementCall (prevCall, expr) ->
                        let subPropertyType = generateMulticallBody(prevCall, thisType)
                        generateMethodBody expr
                        let elementType = subPropertyType.GetElementType()
                        emitter.LoadElement(elementType) |> ignore
                        elementType
                    | _ -> failwith "Unknown multicall expression"

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
                        | (x) when x = typeof<int> || x = typeof<float> -> true
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
                    | _ -> failwith "Unknown operator"
                | Ast.LiteralExpression (literal) ->
                    match literal with
                    | Ast.Int32Literal (v) ->
                        emitter.LoadConstant(v) |> ignore
                    | Ast.DoubleLiteral (v) ->
                        emitter.LoadConstant(v) |> ignore
                    | _ -> failwith "Unknown literal"
                | Ast.TimeSpanExpression (expr) ->
                    generateMethodBody expr
                    let fromHoursMethod = typeof<TimeSpan>.GetMethod("FromHours")
                    emitter.Call(fromHoursMethod) |> ignore
                | Ast.UnaryExpression (uExp, expr) ->
                    match uExp with
                    | Ast.LogicalNegate ->
                        raise(new NotImplementedException("Logical negate is not implemented yet."))
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

    member this.AddEnvironment (environmentClass: Type) : ClassBuilder =
        if resultType <> null then failwith "Object has been already built and cannot be updated after that."
        environmentClasses.Add(environmentClass)
        this

    member this.Setup (property: string, expression: string) :ClassBuilder =
        if resultType <> null then failwith "Object has been already built and cannot be updated after that."

        let propertyInfo = getProperties(targetType) |> Array.find (fun x -> x.Name = property) // todo: add an exception here
        propertyExpressions.Add({ Property= propertyInfo; Expr = expression } )
        this

    member this.BuildObject ([<ParamArray>] parameters: Object[]):obj =
        if resultType = null then
            resultType <- createType targetType
        Activator.CreateInstance(resultType, parameters)

type public ClassBuilder<'T when 'T: null>() =
    inherit ClassBuilder(typeof<'T>)

    member this.AddEnvironment (environmentClass: Type) :ClassBuilder<'T> =
        base.AddEnvironment(environmentClass) :?> ClassBuilder<'T>

    member this.Setup<'TProperty> (property:System.Linq.Expressions.Expression<Func<'T, 'TProperty>>, expression: string):ClassBuilder<'T> = 
        let body = property.Body :?> System.Linq.Expressions.MemberExpression
        base.Setup(body.Member.Name, expression) :?> ClassBuilder<'T>

    member this.BuildObject ([<ParamArray>] parameters: Object[]):'T = 
        base.BuildObject(parameters) :?> 'T

