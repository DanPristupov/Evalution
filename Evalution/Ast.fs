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

and IdentifierRef = // TODO: Why IdentifierRef? Rename to Identifier
    | Identifier of string

and Arguments = Expression list

and Multicall =
    | CurrentContextMethodCall of IdentifierRef * Arguments
    | CurrentContextPropertyCall of IdentifierRef
    | ObjectContextPropertyCall of Multicall * IdentifierRef
    | ArrayElementCall of Multicall * Expression

and UnaryOperator =
    | LogicalNegate
    | Negate
    | Identity