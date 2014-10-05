module MainModule
    open System
    open Evalution

    [<EntryPoint>]
    let main argv = 
        
        let result = AstBuilder.build("a+b")
        printfn "%A" result
        Console.ReadLine()
        0 // return an integer exit code
