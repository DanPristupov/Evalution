module Evalution.Ast
type Program = Expression

and Expression =
    | LiteralExpression of Literal
    | BinaryExpression of Expression * BinaryOperator * Expression
and Literal =
    | BoolLiteral of bool
    | Int32Literal of int
    | DoubleLiteral of float

and BinaryOperator =
    | Add
    | Subtract
    | Multiply
    | Divide