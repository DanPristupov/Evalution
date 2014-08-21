module MainModule
    open System
    open Evalution

    [<EntryPoint>]
    let main argv = 
        let syntaxTree = new SyntaxTree()
        let result = syntaxTree.Build [
            Integer 2;
            Operator '+';
            Integer 3;
        ]
        
        let tokenizer = new Tokenizer()
        let result = tokenizer.Read("(42+8) *2")
        result |> Seq.iter ( printfn "%A")

        Console.ReadLine()
        0 // return an integer exit code
