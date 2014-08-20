namespace Evalution
open System

type Const =
    | CDouble of float
    | CInteger of int

type Expression = 
    | Const of Const
    | Addition of (Expression * Expression)
    | Multiplication of (Expression * Expression)
    | None

type public SyntaxTree() =
    let build tokens =
        let mutable result = []

        for token in tokens do
            match token with
            | Double(value) -> result <- (Const (CDouble value)) :: result
            | Integer(value) -> result <- (Const (CInteger value)) :: result
            | Operator(value) -> match value with
                                    | '*' -> result <- (Multiplication ((Const (CInteger 1)), (Const (CInteger 1)))) :: result
                                    | '+' -> result <- (Addition ((Const (CInteger 1)), (Const (CInteger 1)))) :: result
                                    | _ -> failwith ""
            | _ -> failwith ""
        result.Head

    member this.Build tokens = build