namespace Evalution
open System

type public EvalutionException(message : string) =
    inherit System.Exception(message)

type public InvalidNameException(message: string) =
    inherit EvalutionException(message)

module EvalutionErrors =
    let evalutionException m = EvalutionException m
    let invalidNameException m = InvalidNameException m

    let invalidNameError n = invalidNameException(sprintf "Cannot find name '%s' in the current context." n)



