using System;
using System.Collections.Generic;

namespace Evalution.Ast
{
    using Piglet.Parser;

    public class AstBuilder
    {
        public static Expression Build(string input)
        {
            var configurator = ParserFactory.Configure<object>();

            var expressionSpec = configurator.CreateNonTerminal();
            var multiCallExpressionSpec = configurator.CreateNonTerminal();
            var unaryExpressionSpec = configurator.CreateNonTerminal();
            var argumentsSpec = configurator.CreateNonTerminal();
            var optionalArgumentsSpec = configurator.CreateNonTerminal();

            var int32Literal = configurator.CreateTerminal(@"\d+", x => new Int32Literal(Int32.Parse(x)));
            var doubleLiteral = configurator.CreateTerminal(@"\d+\.\d+", x => new DoubleLiteral(Double.Parse(x)) );
            var plus = configurator.CreateTerminal(@"\+");
            var minus = configurator.CreateTerminal(@"-");
            var exclamation = configurator.CreateTerminal(@"!");
            var asterisk = configurator.CreateTerminal(@"\*");
            var forwardSlash = configurator.CreateTerminal(@"/");
            var dot = configurator.CreateTerminal(@"\.");
            var comma = configurator.CreateTerminal(@",");
            var openSquare = configurator.CreateTerminal(@"\[");
            var closeSquare = configurator.CreateTerminal(@"\]");
            var openParen = configurator.CreateTerminal(@"\(");
            var closeParen = configurator.CreateTerminal(@"\)");
            var identifier = configurator.CreateTerminal(@"[a-zA-Z_][a-zA-Z_0-9]*", x => x);
            var timeSpan = configurator.CreateTerminal(@"TimeSpan\.FromHours");

            configurator.LeftAssociative(plus, minus, exclamation);
            configurator.LeftAssociative(asterisk);
            configurator.LeftAssociative(forwardSlash);
            configurator.LeftAssociative(dot);
            var unaryExpressionPrecedenceGroup = configurator.RightAssociative();

            // Parens
            expressionSpec.AddProduction(openParen, expressionSpec, closeParen)
                .SetReduceFunction(x => (Expression)x[1]);

            // BinaryExpressions
            expressionSpec.AddProduction(expressionSpec, plus, expressionSpec)
                .SetReduceFunction(x => new BinaryExpression((Expression)x[0], BinaryOperator.Add, (Expression)x[2]));
            expressionSpec.AddProduction(expressionSpec, minus, expressionSpec)
                .SetReduceFunction(x => new BinaryExpression((Expression)x[0], BinaryOperator.Subtract, (Expression)x[2]));
            expressionSpec.AddProduction(expressionSpec, asterisk, expressionSpec)
                .SetReduceFunction(x => new BinaryExpression((Expression)x[0], BinaryOperator.Multiply, (Expression)x[2]));
            expressionSpec.AddProduction(expressionSpec, forwardSlash, expressionSpec)
                .SetReduceFunction(x => new BinaryExpression((Expression)x[0], BinaryOperator.Divide, (Expression)x[2]));

            // Literals
            expressionSpec.AddProduction(int32Literal)
                .SetReduceFunction(x => new LiteralExpression((Literal)x[0]) );
            expressionSpec.AddProduction(doubleLiteral)
                .SetReduceFunction(x => new LiteralExpression((Literal)x[0]) );

            // UnaryOperators
            var expressionProduction = expressionSpec.AddProduction(unaryExpressionSpec, expressionSpec);
            expressionProduction.SetReduceFunction(x => new UnaryExpression((UnaryOperator)x[0], (Expression)x[1]));
            expressionProduction.SetPrecedence(unaryExpressionPrecedenceGroup);

            unaryExpressionSpec.AddProduction(exclamation)
                .SetReduceFunction(x => UnaryOperator.LogicalNegate);
            unaryExpressionSpec.AddProduction(minus)
                .SetReduceFunction(x => UnaryOperator.Negate);
            unaryExpressionSpec.AddProduction(plus)
                .SetReduceFunction(x => UnaryOperator.Identity);

            // Multicall expression
            expressionSpec.AddProduction(multiCallExpressionSpec)
                .SetReduceFunction(x => new MultiCallExpression((Multicall) x[0]));

             // Default context method call
            multiCallExpressionSpec.AddProduction(identifier, openParen, optionalArgumentsSpec, closeParen)
                .SetReduceFunction(x => new CurrentContextMethodCall(
                    (string) x[0],
                    (List<Expression>) x[2]));

            // Object context method call
            multiCallExpressionSpec.AddProduction(multiCallExpressionSpec, dot, identifier, openParen, optionalArgumentsSpec, closeParen)
                .SetReduceFunction(x => new ObjectContextMethodCall(
                                                    (Multicall)x[0],
                                                    (string)x[2],
                                                    (List<Expression>)x[4]));

            // Property call
            multiCallExpressionSpec.AddProduction(identifier)
                .SetReduceFunction(x => new CurrentContextPropertyCall((string) x[0]));

            // Subproperty call
            multiCallExpressionSpec.AddProduction(multiCallExpressionSpec, dot, identifier)
                .SetReduceFunction(x => new ObjectContextPropertyCall((Multicall) x[0], (string) x[2]));

            // Array element call
            multiCallExpressionSpec.AddProduction(multiCallExpressionSpec, openSquare, expressionSpec, closeSquare)
                .SetReduceFunction(x => new ArrayElementCall(
                                                    (Multicall)x[0],
                                                    (Expression)x[2]));

            optionalArgumentsSpec.AddProduction(argumentsSpec).SetReduceToFirst();
            optionalArgumentsSpec.AddProduction()
                .SetReduceFunction(x =>new List<Expression>());

            argumentsSpec.AddProduction(argumentsSpec, comma, expressionSpec)
                .SetReduceFunction(x => AddElementToList((List<Expression>)x[0], (Expression)x[2]));

            argumentsSpec.AddProduction(expressionSpec)
                .SetReduceFunction(x => new List<Expression>() { (Expression)x[0] });

            var parser = configurator.CreateParser();
            var result = parser.Parse(input);

            return (Expression)result;
        }

        private static List<Expression> AddElementToList(List<Expression> expressions, Expression expression)
        {
            expressions.Add(expression);
            return expressions;
        }
    }
}
