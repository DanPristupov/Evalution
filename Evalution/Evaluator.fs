namespace Evalution

open System.Reflection.Emit
open System
open Sigil

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
        // I stopped here
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

    let rec compile expr = 
        let emiter = Emit<Func<float>>.NewDynamicMethod("EvaluatorMethod");
        emiter.DeclareLocal(typeof<float>)
        generateMethodBody(emiter, expr)
        emiter.Return();
        let action = emiter.CreateDelegate()

        action.Invoke()

    let buildObject (objType:Type)=
        let properties =
            objType.GetProperties()
            |> Seq.map(fun x -> (x, x.GetCustomAttributes(typeof<ExpressionAttribute>, true)))
            |> Seq.filter(fun (prop, attributes) -> attributes.Length = 1 )
            |> Seq.map(fun (prop, attributes) -> (prop, attributes |> Seq.head) )

        for (property, attribute) in properties do
            printfn "%s" property.Name
        Activator.CreateInstance(objType)

    member this.Compile (expr:Expression): float = compile expr
    member this.Evaluate (expr:Expression) = evaluate expr
    member this.BuildObject<'T when 'T: null> () :'T = (buildObject typeof<'T>) :?> 'T