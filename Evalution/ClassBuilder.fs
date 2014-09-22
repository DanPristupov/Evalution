namespace Evalution
open System

type public ClassBuilder(targetType:Type) =

    member this.BuildObject ():obj = 
        null

type public ClassBuilder<'T when 'T: null>() =
    inherit ClassBuilder(typeof<'T>)

    member this.Setup<'TProperty> (property:Func<'T, 'TProperty>, expression: string) = 
        null

    member this.BuildObject ():'T = 
        base.BuildObject() :?> 'T