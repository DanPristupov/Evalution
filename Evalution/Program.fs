module MainModule
    open System
    open Evalution

    [<EntryPoint>]
    let main argv = 
        
        let aa = AstBuilder.build("a+b")












        let tokenizer = new Tokenizer()
        let tokens = tokenizer.Read("(42+8) *2")
        tokens |> Seq.iter ( printfn "%A")

        let syntaxTree = new SyntaxTree()
        let expression = syntaxTree.Build tokens
        
        let evaluator = new Evaluator();
        let compiledResult = evaluator.Compile(expression);
        printfn "%A" compiledResult
        let result = evaluator.Evaluate(expression);
        printfn "%A" result
        Console.ReadLine()
        0 // return an integer exit code
