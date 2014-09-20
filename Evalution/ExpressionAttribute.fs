namespace Evalution
open System

type public ExpressionAttribute(expression: string) =
    inherit Attribute()

    member val Expression = expression with get, set