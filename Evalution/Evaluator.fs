namespace Evalution

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

    member this.Evaluate (expr:Expression) = evaluate expr