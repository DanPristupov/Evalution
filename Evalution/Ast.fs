module Evalution.Ast

open System

type Program = Expression

and Expression =
    | BinaryExpression of Expression * BinaryOperator * Expression
    | LiteralExpression of Literal
    | UnaryExpression of UnaryOperator * Expression
    | MultiCallExpression of Multicall

and Literal =
    | BoolLiteral of bool
    | Int32Literal of int
    | DoubleLiteral of float
    | TimeSpanLiteral of TimeSpan // TODO: make of long (ticks)

and BinaryOperator =
    | Add
    | Subtract
    | Multiply
    | Divide

and IdentifierRef =
    | Identifier of string
    
and Multicall =
    | ThisPropertyCall of IdentifierRef
    | ObjectPropertyCall of Multicall * IdentifierRef
    | ArrayElementCall of Multicall * Expression

and UnaryOperator =
    | LogicalNegate
    | Negate
    | Identity