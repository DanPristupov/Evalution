﻿namespace Evalution
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

// rename to SyntaxTreeBuilder?
type public SyntaxTree() =

    let build tokens =
        let convert (operation, stack:Stack<Expression>) =
            match operation with
            | '+' -> Addition(stack.Pop(), stack.Pop())
            | '*' -> Multiplication(stack.Pop(), stack.Pop())
            | _ -> failwith "blah"
        let getOperator (Operator token) = token

        let popStackTokens (tokenStack:Stack<Token>, resultStack:Stack<Expression>)=
            let isLeftBracket token =
                match token with
                | Bracket(value) -> match value with
                                        | '(' -> true
                                        | _ -> false
                | _ -> false


            while tokenStack.Count > 0 && not(isLeftBracket(tokenStack.Peek())) do
                let operation = tokenStack.Pop()
                resultStack.Push( convert((getOperator operation), resultStack))
            ()

        let getPriority token =
            match token with
            | Bracket(_) -> 0
            | Operator(value) -> match value with
                                 | '+' -> 1
                                 | '*' -> 2
            | Double(_) | Integer(_) -> 0
            | _ -> failwith ""

        let mutable resultStack = new Stack<Expression>()
        let mutable tokenStack = new Stack<Token>()

        let rec processTokens tokens (resultStack: Stack<Expression>) (tokenStack:Stack<Token>)=
            match tokens with
            | token::tail ->
                match token with
                    | Double(value) -> resultStack.Push (Const (CDouble value))
                    | Integer(value) -> resultStack.Push (Const (CInteger value))
                    | Operator(value) -> match value with
                                            | '*' | '+' -> if tokenStack.Count = 0 then
                                                                tokenStack.Push token
                                                            else
                                                                let operation2 = tokenStack.Peek()
                                                                if (getPriority token) <= (getPriority operation2) then
                                                                    tokenStack.Pop()
                                                                    tokenStack.Push(token)
                                                                    let operator = getOperator operation2
                                                                    let value = convert ( operator, resultStack)
                                                                    resultStack.Push(value)
                                                                else
                                                                    tokenStack.Push(token)
                                            | _ -> failwith ""
                    | Bracket(value) -> match value with
                                            | '(' -> tokenStack.Push(token)
                                            | ')' -> popStackTokens(tokenStack, resultStack)
                                            | _ -> failwith ""
                    | _ -> failwith ""
                processTokens tail resultStack tokenStack
            | [] -> ()

        processTokens tokens resultStack tokenStack
        popStackTokens(tokenStack, resultStack)
        Seq.nth 0 resultStack

    member this.Build tokens = build tokens