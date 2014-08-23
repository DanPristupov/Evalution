namespace Evalution
open System
open System.Collections.Generic

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
        let popStackTokens (tokenStack:Stack<Token>, resultStack:Stack<Expression>)=
            let isLeftBracket token =
                match token with
                | Bracket(value) -> match value with
                                        | '(' -> true
                                        | _ -> false
                | _ -> false

            let getOperator (Operator token) = token
            let convert (operation, stack:Stack<Expression>) =
                match operation with
                | '+' -> Addition(stack.Pop(), stack.Pop())
                | '*' -> Multiplication(stack.Pop(), stack.Pop())
                | _ -> failwith "blah"

            while tokenStack.Count > 0 && not(isLeftBracket(tokenStack.Peek())) do
                let operation = tokenStack.Pop()
                resultStack.Push( convert((getOperator operation), resultStack))
            ()

        let mutable resultStack = new Stack<Expression>()
        let mutable tokenStack = new Stack<Token>()

        let mutable result = []


        for token in tokens do
            match token with
            | Double(value) -> resultStack.Push (Const (CDouble value))
            | Integer(value) -> resultStack.Push (Const (CInteger value))
            | Operator(value) -> match value with
                                    | '*' | '+' -> if tokenStack.Count = 0 then
                                                    tokenStack.Push token
                                                    else
                                                    let operation2 = tokenStack.Peek()
                                                    tokenStack.Pop()
                                                    tokenStack.Push(token)
                                                    //resultStack.Push bla bla
                                    | _ -> failwith ""
            | Bracket(value) -> match value with
                                    | '(' -> tokenStack.Push(token)
                                    | ')' -> popStackTokens(tokenStack, resultStack)
                                    | _ -> failwith ""
            | _ -> failwith ""
        //PopOperations(operatorStack, resultStack);
        popStackTokens(tokenStack, resultStack)
        Seq.nth 0 resultStack

    member this.Build tokens = build tokens