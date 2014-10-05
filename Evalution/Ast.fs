module Evalution.Ast
type Program = Expression

and Expression =
    | BinaryExpression of Expression * BinaryOperator * Expression
    | LiteralExpression of Literal
    | MultiCallExpression of MultiCall

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
    
and MultiCall =
    | ThisPropertyCall of IdentifierRef
    | ObjectPropertyCall of MultiCall * IdentifierRef