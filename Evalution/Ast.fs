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

and Identifier = string

and Arguments = Expression list

and Multicall =
    | CurrentContextMethodCall of Identifier * Arguments
    | CurrentContextPropertyCall of Identifier
    | ObjectContextPropertyCall of Multicall * Identifier
    | ArrayElementCall of Multicall * Expression

and UnaryOperator =
    | LogicalNegate
    | Negate
    | Identity