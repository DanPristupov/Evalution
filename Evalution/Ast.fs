module Evalution.Ast
type Program = Expression

and Expression =
    | BinaryExpression of Expression * BinaryOperator * Expression
    | LiteralExpression of Literal
    | IdentifierExpression of IdentifierRef
    | PropertyCallExpression of PropertyCall

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
    
and PropertyCall =
    | ThisPropertyCall of string
    | ObjectPropertyCall of PropertyCall * string