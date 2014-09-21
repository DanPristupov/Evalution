namespace Evalution

open System.Reflection
open System.Reflection.Emit
open System
open Sigil
open Sigil.NonGeneric

type public Evaluator() =
    let rec evaluate expr = 
        match expr with
        | Addition (l, r) -> (evaluate l) + (evaluate r)
        | Multiplication (l, r) -> (evaluate l) * (evaluate r)
        | Const(value) -> match value with
                        | CDouble(v) -> v
                        | CInteger(v) -> (float v)
                        | _ -> failwith "blah"

    let rec generateMethodBody (emiter:Emit<Func<float>>, expr) =
        match expr with
        | Const(value) -> match value with
                | CDouble(v) ->
                    emiter.LoadConstant(v)
                | CInteger(v) ->
                    emiter.LoadConstant(v)
                    emiter.Convert(typeof<float>)
                | _ -> failwith "blah"
        |Addition (l, r) ->
            generateMethodBody(emiter, l)
            generateMethodBody(emiter, r)
            emiter.Add()
        |Multiplication (l, r) ->
            generateMethodBody(emiter, l)
            generateMethodBody(emiter, r)
            emiter.Multiply()
        | _ -> failwith "blah"
        ()

    let rec generateMethodBodyInt (emiter:Emit, expr, allProperties: PropertyInfo[]) =
        match expr with
        | Const(value) -> match value with
                | CDouble(v) ->
                    emiter.LoadConstant(v)
                | CInteger(v) ->
                    emiter.LoadConstant(v)
                | _ -> failwith "blah"
        |Property (name) ->
            let property = allProperties |> Seq.find(fun x -> x.Name = name)
            let getMethod = property.GetGetMethod()
            emiter.LoadArgument(uint16 0)
            emiter.CallVirtual(getMethod)
        |Addition (l, r) ->
            generateMethodBodyInt(emiter, l, allProperties)
            generateMethodBodyInt(emiter, r, allProperties)
            emiter.Add()
        |Multiplication (l, r) ->
            generateMethodBodyInt(emiter, l, allProperties)
            generateMethodBodyInt(emiter, r, allProperties)
            emiter.Multiply()
        | _ -> failwith "blah"
        ()
    let rec compile expr = 
        let emiter = Emit<Func<float>>.NewDynamicMethod("EvaluatorMethod");
        emiter.DeclareLocal(typeof<float>)
        generateMethodBody(emiter, expr)
        emiter.Return();
        let action = emiter.CreateDelegate()

        action.Invoke()

    let createTypeBuilder (objType:Type) =
        let typeSignature = "myType"
        let assemblyName = new AssemblyName(typeSignature) // may be I should use assembly of the objType?
        let assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run)
        let moduleBuilder = assemblyBuilder.DefineDynamicModule("myTypeModule");
        let typeBuilder = moduleBuilder.DefineType(typeSignature,
                                TypeAttributes.Public |||
                                TypeAttributes.Class |||
                                TypeAttributes.AutoClass |||
                                TypeAttributes.AnsiClass |||
                                TypeAttributes.BeforeFieldInit |||
                                TypeAttributes.AutoLayout,
                                null)
        typeBuilder

    let buildObject (objType:Type)=

        let createType (objType: Type, props):Type =
            let typeBuilder = createTypeBuilder objType
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public |||
                                                 MethodAttributes.SpecialName |||
                                                 MethodAttributes.RTSpecialName) |> ignore
            typeBuilder.SetParent(objType)

            let createGetPropertyMethodBuilder(propertyName, propertyType:Type, fieldBuilder, attribute:ExpressionAttribute, allProperties):MethodBuilder =
                let emitter = Emit.BuildMethod(propertyType,Array.empty,typeBuilder, "get_"+propertyName, MethodAttributes.Public ||| MethodAttributes.SpecialName ||| MethodAttributes.HideBySig ||| MethodAttributes.Virtual,CallingConventions.Standard ||| CallingConventions.HasThis)
                let tokenizer = new Tokenizer()
                let syntaxTree = new SyntaxTree()

                generateMethodBodyInt(emitter, (syntaxTree.Build (tokenizer.Read attribute.Expression)), allProperties)
                emitter.Return()
                emitter.CreateMethod()

            let createProperty (property:PropertyInfo, attribute:ExpressionAttribute, allProperties) =
                let fieldBuilder = typeBuilder.DefineField("_" + property.Name, property.PropertyType, FieldAttributes.Private);
                let propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.PropertyType, null);
                
                let getMethodBuilder = createGetPropertyMethodBuilder(property.Name, property.PropertyType, fieldBuilder, attribute, allProperties)
                propertyBuilder.SetGetMethod(getMethodBuilder);

                ()
            let allProperties = objType.GetProperties()
            for (property, attribute) in props do
                createProperty(property, attribute, allProperties)

            typeBuilder.CreateType()

        let properties =
            objType.GetProperties()
            |> Seq.map(fun x -> (x, x.GetCustomAttributes(typeof<ExpressionAttribute>, true)))
            |> Seq.filter(fun (prop, attributes) -> attributes.Length = 1 )
            |> Seq.map(fun (prop, attributes) -> (prop, attributes |> Seq.head :?>ExpressionAttribute) )
            |> Seq.toArray

        let resultType = createType (objType, properties)

        Activator.CreateInstance(resultType)

    member this.Compile (expr:Expression): float = compile expr
    member this.Evaluate (expr:Expression) = evaluate expr
    member this.BuildObject<'T when 'T: null> () :'T = (buildObject typeof<'T>) :?> 'T