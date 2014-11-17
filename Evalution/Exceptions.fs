namespace Evalution
open System

type public ExpressionParsingException1(message: string) =
    inherit Exception(message)

type public InvalidNameException(message: string, name: string) =
    inherit Exception(message)

