module Evalution.AstBuilder
    open Piglet.Parser

    let build (input:string) : Ast.Program =
        let configurator = ParserFactory.Configure<obj>()

        let expressionSpec = configurator.CreateNonTerminal()
        let multiCallExpressionSpec = configurator.CreateNonTerminal()

        let int32Literal = configurator.CreateTerminal(@"\d+", fun x -> box (Ast.Int32Literal (int x) ) )
        let doubleLiteral = configurator.CreateTerminal(@"\d+\.\d+", fun x -> box (Ast.DoubleLiteral (float x) ) )
        let plus = configurator.CreateTerminal(@"\+")
        let minus = configurator.CreateTerminal(@"-")
        let asterisk = configurator.CreateTerminal(@"\*")
        let forwardSlash = configurator.CreateTerminal(@"/")
        let dot = configurator.CreateTerminal(@"\.")
        let identifier = configurator.CreateTerminal(@"[a-zA-Z_][a-zA-Z_0-9]*", fun x -> box(Ast.Identifier(x)))

        configurator.LeftAssociative(plus)
        configurator.LeftAssociative(minus)
        configurator.LeftAssociative(asterisk)
        configurator.LeftAssociative(forwardSlash)
        configurator.LeftAssociative(dot)

        expressionSpec.AddProduction(expressionSpec, plus, expressionSpec)
            .SetReduceFunction(fun x -> box (Ast.BinaryExpression(x.[0] :?> Ast.Expression, Ast.Add, x.[2] :?>Ast.Expression)))
        expressionSpec.AddProduction(expressionSpec, minus, expressionSpec)
            .SetReduceFunction(fun x -> box (Ast.BinaryExpression(x.[0] :?> Ast.Expression, Ast.Subtract, x.[2] :?>Ast.Expression)))
        expressionSpec.AddProduction(expressionSpec, asterisk, expressionSpec)
            .SetReduceFunction(fun x -> box (Ast.BinaryExpression(x.[0] :?> Ast.Expression, Ast.Multiply, x.[2] :?>Ast.Expression)))
        expressionSpec.AddProduction(expressionSpec, forwardSlash, expressionSpec)
            .SetReduceFunction(fun x -> box (Ast.BinaryExpression(x.[0] :?> Ast.Expression, Ast.Divide, x.[2] :?>Ast.Expression)))

        expressionSpec.AddProduction(int32Literal)
            .SetReduceFunction(fun x -> box (Ast.LiteralExpression(x.[0]:?>Ast.Literal ) ))
        expressionSpec.AddProduction(doubleLiteral)
            .SetReduceFunction(fun x -> box (Ast.LiteralExpression(x.[0]:?>Ast.Literal ) ))

        expressionSpec.AddProduction(multiCallExpressionSpec)
            .SetReduceFunction(fun x -> box (Ast.MultiCallExpression(x.[0] :?> Ast.MultiCall)) )

        multiCallExpressionSpec.AddProduction(identifier)
            .SetReduceFunction(fun x -> box (Ast.ThisPropertyCall(x.[0] :?> Ast.IdentifierRef)) )
        multiCallExpressionSpec.AddProduction(multiCallExpressionSpec, dot, identifier)
            .SetReduceFunction(fun x -> box (Ast.ObjectPropertyCall(x.[0] :?>Ast.MultiCall, x.[2] :?>Ast.IdentifierRef)) )

        let parser = configurator.CreateParser()
        let result = parser.Parse(input)

        result :?> Ast.Program
        