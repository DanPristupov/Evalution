module Evalution.AstBuilder
    open System
    open Piglet.Parser

    let build (input:string) : Ast.Program =
        let getTimeSpan (input:string) :TimeSpan =
            let value = input.Substring(1 + input.IndexOf '(').TrimEnd(')')
            TimeSpan.FromHours(float value)

        let configurator = ParserFactory.Configure<obj>()

        let expressionSpec = configurator.CreateNonTerminal()
        let multiCallExpressionSpec = configurator.CreateNonTerminal()
        let unaryExpressionSpec = configurator.CreateNonTerminal()

        let int32Literal = configurator.CreateTerminal(@"\d+", fun x -> box (Ast.Int32Literal (int x) ) )
        let doubleLiteral = configurator.CreateTerminal(@"\d+\.\d+", fun x -> box (Ast.DoubleLiteral (float x) ) )
        let timeSpanLiteral = configurator.CreateTerminal(
                                            @"TimeSpan\.FromHours\(\d+\.\d+\)",
                                            fun x -> box (Ast.TimeSpanLiteral (getTimeSpan(x)) ) )
        let plus = configurator.CreateTerminal(@"\+")
        let minus = configurator.CreateTerminal(@"-")
        let exclamation = configurator.CreateTerminal(@"!")
        let asterisk = configurator.CreateTerminal(@"\*")
        let forwardSlash = configurator.CreateTerminal(@"/")
        let dot = configurator.CreateTerminal(@"\.")
        let openSquare = configurator.CreateTerminal(@"\[")
        let closeSquare = configurator.CreateTerminal(@"\]")
        let identifier = configurator.CreateTerminal(@"[a-zA-Z_][a-zA-Z_0-9]*", fun x -> box(Ast.Identifier(x)))

        configurator.LeftAssociative(plus, minus, exclamation) |> ignore
        configurator.LeftAssociative(asterisk) |> ignore
        configurator.LeftAssociative(forwardSlash) |> ignore
        configurator.LeftAssociative(dot) |> ignore
        let unaryExpressionPrecedenceGroup  = configurator.RightAssociative()

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
        expressionSpec.AddProduction(timeSpanLiteral)
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

        multiCallExpressionSpec.AddProduction(identifier)
            .SetReduceFunction(fun x -> box (Ast.ThisPropertyCall(x.[0] :?> Ast.IdentifierRef)) )
        multiCallExpressionSpec.AddProduction(multiCallExpressionSpec, dot, identifier)
            .SetReduceFunction(fun x -> box (Ast.ObjectPropertyCall(x.[0] :?>Ast.Multicall, x.[2] :?>Ast.IdentifierRef)) )

        multiCallExpressionSpec.AddProduction(multiCallExpressionSpec, openSquare, expressionSpec, closeSquare)
            .SetReduceFunction(fun x -> box (Ast.ArrayElementCall(
            x.[0] :?> Ast.Multicall,
            x.[2] :?> Ast.Expression
            )) )


        let parser = configurator.CreateParser()
        let result = parser.Parse(input)

        result :?> Ast.Program
        