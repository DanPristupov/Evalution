namespace Evalution

open System.Reflection.Emit
open System

type public Evaluator() =
    let getConst (Const value) = value

    let rec evaluate expr = 
        match expr with
        | Addition (l, r) -> (evaluate l) + (evaluate r)
        | Multiplication (l, r) -> (evaluate l) * (evaluate r)
        | Const(value) -> match value with
                        | CDouble(v) -> v
                        | CInteger(v) -> (float v)
                        | _ -> failwith "blah"

    let rec generateMethodBody (ilGen:ILGenerator, expr) =
        // I stopped here
        match expr with
        | Const(value) -> match value with
                | CDouble(v) ->
                    ilGen.Emit(OpCodes.Ldc_R8, v)                    
                | CInteger(v) ->
                    ilGen.Emit(OpCodes.Ldc_I4, v)
                    ilGen.Emit(OpCodes.Conv_R8)
                | _ -> failwith "blah"
        |Addition (l, r) ->
            generateMethodBody(ilGen, l)
            generateMethodBody(ilGen, r)
            ilGen.Emit(OpCodes.Add)
        |Multiplication (l, r) ->
            generateMethodBody(ilGen, l)
            generateMethodBody(ilGen, r)
            ilGen.Emit(OpCodes.Mul)
        | _ -> failwith "blah"
        ()

    let rec compile expr = 
        let dynamicMethod = new DynamicMethod("EvaluatorMethod", typeof<float>, null)
        let ilGen = dynamicMethod.GetILGenerator();
        ilGen.DeclareLocal(typeof<float>)
        generateMethodBody(ilGen, expr)
        ilGen.Emit(OpCodes.Ret)

        let action = dynamicMethod.CreateDelegate(typeof<Func<float>>) :?> Func<float>
        action.Invoke()

    member this.Compile (expr:Expression): float = compile expr
    member this.Evaluate (expr:Expression) = evaluate expr