namespace Evalution
open System

type public ClassBuilder(targetType:Type) =
    member this.Setup (property: string, expression: string) :ClassBuilder= 
        this

    member this.BuildObject ():obj = 
        null

type public ClassBuilder<'T when 'T: null>() =
    inherit ClassBuilder(typeof<'T>)

    member this.Setup<'TProperty> (property:Func<'T, 'TProperty>, expression: string):ClassBuilder<'T> = 
        this

    member this.BuildObject ():'T = 
        base.BuildObject() :?> 'T