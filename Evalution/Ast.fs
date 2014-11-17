module Evalution.Ast

type Program = Expression

and Expression =
    | BinaryExpression of Expression * BinaryOperator * Expression
    | LiteralExpression of Literal
    | UnaryExpression of UnaryOperator * Expression
    | MultiCallExpression of Multicall
    | TimeSpanExpression of Expression // TODO: make of long (ticks)

and Literal =
    | BoolLiteral of bool
    | Int32Literal of int
    | DoubleLiteral of float

and BinaryOperator =
    | Add
    | Subtract
    | Multiply
    | Divide

and IdentifierRef =
    | Identifier of string
    
and Multicall =
    | CurrentContextPropertyCall of IdentifierRef
    | ObjectPropertyCall of Multicall * IdentifierRef
    | ArrayElementCall of Multicall * Expression

and UnaryOperator =
    | LogicalNegate
    | Negate
    | Identity