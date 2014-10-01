﻿module MainModule
    open System
    open Evalution

    open Piglet.Parser

    [<EntryPoint>]
    let main argv = 
        
        let configurator = ParserFactory.Configure<obj>()

        let expressionSpec = configurator.CreateNonTerminal()

        let int32Literal = configurator.CreateTerminal(@"\d+", fun x -> box (Ast.Int32Literal (int x) ) )
        let doubleLiteral = configurator.CreateTerminal(@"\d+\.\d+", fun x -> box (Ast.DoubleLiteral (float x) ) )
        let plus = configurator.CreateTerminal(@"\+")

        configurator.LeftAssociative(plus)

        expressionSpec.AddProduction(expressionSpec, plus, expressionSpec)
            .SetReduceFunction(fun x -> box (Ast.BinaryExpression(x.[0] :?> Ast.Expression, Ast.Add, x.[2] :?>Ast.Expression)))

        expressionSpec.AddProduction(int32Literal)
            .SetReduceFunction(fun x -> box (Ast.LiteralExpression(x.[0]:?>Ast.Literal ) ))
        expressionSpec.AddProduction(doubleLiteral)
            .SetReduceFunction(fun x -> box (Ast.LiteralExpression(x.[0]:?>Ast.Literal ) ))

        expressionSpec.AddProduction(plus)
            .SetReduceToFirst()

        let parser = configurator.CreateParser()
        let aa = parser.Parse("23+20.2")












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
