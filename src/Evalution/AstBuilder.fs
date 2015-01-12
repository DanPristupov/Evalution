module Evalution.AstBuilder
    open Piglet.Parser

    let build (input:string) : Ast.Program =
        let configurator = ParserFactory.Configure<obj>()

        let expressionSpec = configurator.CreateNonTerminal()
        let multiCallExpressionSpec = configurator.CreateNonTerminal()
        let unaryExpressionSpec = configurator.CreateNonTerminal()
        let argumentsSpec = configurator.CreateNonTerminal()
        let optionalArgumentsSpec = configurator.CreateNonTerminal()

        let int32Literal = configurator.CreateTerminal(@"\d+", fun x -> box (Ast.Int32Literal (int x) ) )
        let doubleLiteral = configurator.CreateTerminal(@"\d+\.\d+", fun x -> box (Ast.DoubleLiteral (float x) ) )
        let plus = configurator.CreateTerminal(@"\+")
        let minus = configurator.CreateTerminal(@"-")
        let exclamation = configurator.CreateTerminal(@"!")
        let asterisk = configurator.CreateTerminal(@"\*")
        let forwardSlash = configurator.CreateTerminal(@"/")
        let dot = configurator.CreateTerminal(@"\.")
        let comma = configurator.CreateTerminal(@",")
        let openSquare = configurator.CreateTerminal(@"\[")
        let closeSquare = configurator.CreateTerminal(@"\]")
        let openParen = configurator.CreateTerminal(@"\(")
        let closeParen = configurator.CreateTerminal(@"\)")
        let identifier = configurator.CreateTerminal(@"[a-zA-Z_][a-zA-Z_0-9]*", fun x -> box(x))
        let timeSpan = configurator.CreateTerminal(@"TimeSpan\.FromHours")

        configurator.LeftAssociative(plus, minus, exclamation) |> ignore
        configurator.LeftAssociative(asterisk) |> ignore
        configurator.LeftAssociative(forwardSlash) |> ignore
        configurator.LeftAssociative(dot) |> ignore
        let unaryExpressionPrecedenceGroup  = configurator.RightAssociative()

        // Parens
        expressionSpec.AddProduction(openParen, expressionSpec, closeParen)
            .SetReduceFunction(fun x -> box (x.[1] :?> Ast.Expression))


        // BinaryExpressions
        expressionSpec.AddProduction(expressionSpec, plus, expressionSpec)
            .SetReduceFunction(fun x -> box (Ast.BinaryExpression(x.[0] :?> Ast.Expression, Ast.Add, x.[2] :?>Ast.Expression)))
        expressionSpec.AddProduction(expressionSpec, minus, expressionSpec)
            .SetReduceFunction(fun x -> box (Ast.BinaryExpression(x.[0] :?> Ast.Expression, Ast.Subtract, x.[2] :?>Ast.Expression)))
        expressionSpec.AddProduction(expressionSpec, asterisk, expressionSpec)
            .SetReduceFunction(fun x -> box (Ast.BinaryExpression(x.[0] :?> Ast.Expression, Ast.Multiply, x.[2] :?>Ast.Expression)))
        expressionSpec.AddProduction(expressionSpec, forwardSlash, expressionSpec)
            .SetReduceFunction(fun x -> box (Ast.BinaryExpression(x.[0] :?> Ast.Expression, Ast.Divide, x.[2] :?>Ast.Expression)))

        // Literals
        expressionSpec.AddProduction(int32Literal)
            .SetReduceFunction(fun x -> box (Ast.LiteralExpression(x.[0]:?>Ast.Literal ) ))
        expressionSpec.AddProduction(doubleLiteral)
            .SetReduceFunction(fun x -> box (Ast.LiteralExpression(x.[0]:?>Ast.Literal ) ))

        // UnaryOperators
        let expressionProduction = expressionSpec.AddProduction(unaryExpressionSpec, expressionSpec)
        expressionProduction.SetReduceFunction(fun x -> box(Ast.UnaryExpression(x.[0] :?>Ast.UnaryOperator, x.[1] :?> Ast.Expression)))
        expressionProduction.SetPrecedence unaryExpressionPrecedenceGroup

        unaryExpressionSpec.AddProduction(exclamation)
            .SetReduceFunction(fun x -> box(Ast.LogicalNegate))
        unaryExpressionSpec.AddProduction(minus)
            .SetReduceFunction(fun x -> box(Ast.Negate))
        unaryExpressionSpec.AddProduction(plus)
            .SetReduceFunction(fun x -> box(Ast.Identity))


        // Multicall expression
        expressionSpec.AddProduction(multiCallExpressionSpec)
            .SetReduceFunction(fun x -> box (Ast.MultiCallExpression(x.[0] :?> Ast.Multicall)) )

        // Default context method call
        multiCallExpressionSpec.AddProduction(identifier, openParen, optionalArgumentsSpec, closeParen)
            .SetReduceFunction(fun x -> box (Ast.CurrentContextMethodCall(
                                                x.[0] :?> string,   
                                                x.[2] :?> Ast.Arguments)) )
        // Object context method call
        multiCallExpressionSpec.AddProduction(multiCallExpressionSpec, dot, identifier, openParen, optionalArgumentsSpec, closeParen)
            .SetReduceFunction(fun x -> box (Ast.ObjectContextMethodCall(
                                                x.[0] :?>Ast.Multicall,
                                                x.[2] :?> string,
                                                x.[4] :?> Ast.Arguments)) )

        // Property call
        multiCallExpressionSpec.AddProduction(identifier)
            .SetReduceFunction(fun x -> box (Ast.CurrentContextPropertyCall(x.[0] :?> string)) )

        // Subproperty call
        multiCallExpressionSpec.AddProduction(multiCallExpressionSpec, dot, identifier)
            .SetReduceFunction(fun x -> box (Ast.ObjectContextPropertyCall(x.[0] :?>Ast.Multicall, x.[2] :?> string)) )

        // Array element call
        multiCallExpressionSpec.AddProduction(multiCallExpressionSpec, openSquare, expressionSpec, closeSquare)
            .SetReduceFunction(fun x -> box (Ast.ArrayElementCall(
                                                x.[0] :?> Ast.Multicall,
                                                x.[2] :?> Ast.Expression
                                                )) )
        
        optionalArgumentsSpec.AddProduction(argumentsSpec).SetReduceToFirst()
        optionalArgumentsSpec.AddProduction()
            .SetReduceFunction(fun x -> box (List.empty<Ast.Expression>))

        argumentsSpec.AddProduction(argumentsSpec, comma, expressionSpec)
            .SetReduceFunction(fun x -> box(
                        (x.[0] :?> Ast.Expression list)
                        @
                        ([x.[2] :?> Ast.Expression])
                )
            )
        argumentsSpec.AddProduction(expressionSpec)
            .SetReduceFunction(fun x -> box ([x.[0] :?> Ast.Expression]))

        
        let parser = configurator.CreateParser()
        let result = parser.Parse(input)

        result :?> Ast.Program
        