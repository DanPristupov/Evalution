using System;
using System.Collections.Generic;

namespace EvalutionCS.Ast
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

    public abstract class Expression
    {
    }

    public class BinaryExpression : Expression
    {
        public BinaryExpression(Expression leftExpression, BinaryOperator binaryOperator, Expression rightExpression)
        {
            LeftExpression = leftExpression;
            BinaryOperator = binaryOperator;
            RightExpression = rightExpression;
        }

        public Expression LeftExpression { get; set; }
        public BinaryOperator BinaryOperator { get; set; }
        public Expression RightExpression { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is BinaryExpression)
            {
                var typedObj = obj as BinaryExpression;
                return typedObj.LeftExpression.Equals(LeftExpression)
                       && typedObj.BinaryOperator.Equals(BinaryOperator)
                       && typedObj.RightExpression.Equals(RightExpression);
            }
            return false;
        }

    }

    public class LiteralExpression : Expression
    {
        public LiteralExpression(Literal literal)
        {
            Literal = literal;
        }

        public Literal Literal { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is LiteralExpression)
            {
                var typedObj = obj as LiteralExpression;
                return typedObj.Literal.Equals(Literal);
            }
            return false;
        }
    }
    public class UnaryExpression : Expression
    {
        public UnaryExpression(UnaryOperator unaryOperator, Expression expression)
        {
            UnaryOperator = unaryOperator;
            Expression = expression;
        }

        public UnaryOperator UnaryOperator { get; set; }
        public Expression Expression { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is UnaryExpression)
            {
                var typedObj = obj as UnaryExpression;
                return typedObj.UnaryOperator.Equals(UnaryOperator)
                    && typedObj.Expression.Equals(Expression);
            }
            return false;
        }

    }
    public class MultiCallExpression : Expression
    {
        public MultiCallExpression(Multicall multicall)
        {
            Multicall = multicall;
        }

        public Multicall Multicall { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is MultiCallExpression)
            {
                var typedObj = obj as MultiCallExpression;
                return typedObj.Multicall.Equals(Multicall);
            }
            return false;
        }

    }

    public abstract class Literal
    {
        
    }

    public class BoolLiteral : Literal
    {
        public bool Value { get; set; }
        public override bool Equals(object obj)
        {
            if (obj is BoolLiteral)
            {
                return (obj as BoolLiteral).Value.Equals(Value);
            }
            return false;
        }
    }
    public class Int32Literal : Literal
    {
        public Int32Literal(int value)
        {
            Value = value;
        }

        public int Value { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj is Int32Literal)
            {
                return (obj as Int32Literal).Value.Equals(Value);
            }
            return false;
        }
    }
    public class DoubleLiteral : Literal
    {
        public DoubleLiteral(double value)
        {
            Value = value;
        }

        public double Value { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is DoubleLiteral)
            {
                return (obj as DoubleLiteral).Value.Equals(Value);
            }
            return false;
        }
    }

    public abstract class Multicall
    {
    }

    public class CurrentContextMethodCall : Multicall
    {
        public CurrentContextMethodCall(string identifier, List<Expression> arguments)
        {
            Identifier = identifier;
            Arguments = arguments;
        }

        public string Identifier { get; set; }
        public List<Expression> Arguments { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is CurrentContextMethodCall)
            {
                var typedObj = obj as CurrentContextMethodCall;
                for (var i = 0; i < Arguments.Count; i++)
                {
                    if (!Arguments[i].Equals(typedObj.Arguments[i]))
                    {
                        return false;
                    }
                }
                return typedObj.Identifier.Equals(Identifier);
            }
            return false;
        }

    }
    public class CurrentContextPropertyCall : Multicall
    {
        public CurrentContextPropertyCall(string identifier)
        {
            Identifier = identifier;
        }

        public string Identifier { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is CurrentContextPropertyCall)
            {
                return (obj as CurrentContextPropertyCall).Identifier.Equals(Identifier);
            }
            return false;
        }

    }
    public class ObjectContextPropertyCall : Multicall
    {
        public ObjectContextPropertyCall(Multicall multicall, string identifier)
        {
            Multicall = multicall;
            Identifier = identifier;
        }

        public Multicall Multicall { get; set; }
        public string Identifier { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is ObjectContextPropertyCall)
            {
                return (obj as ObjectContextPropertyCall).Identifier.Equals(Identifier)
                       && (obj as ObjectContextPropertyCall).Multicall.Equals(Multicall);
            }
            return false;
        }

    }
    public class ObjectContextMethodCall : Multicall
    {
        public ObjectContextMethodCall(Multicall multicall, string identifier, List<Expression> arguments)
        {
            Multicall = multicall;
            Identifier = identifier;
            Arguments = arguments;
        }

        public Multicall Multicall { get; set; }
        public string Identifier { get; set; }
        public List<Expression> Arguments { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is ObjectContextMethodCall)
            {
                var typedObj = obj as ObjectContextMethodCall;
                for (var i = 0; i < Arguments.Count; i++)
                {
                    if (!Arguments[i].Equals(typedObj.Arguments[i]))
                    {
                        return false;
                    }
                }
                return typedObj.Identifier.Equals(Identifier)
                    && typedObj.Multicall.Equals(Multicall);
            }
            return false;
        }

    }
    public class ArrayElementCall : Multicall
    {
        public ArrayElementCall(Multicall multicall, Expression expression)
        {
            Multicall = multicall;
            Expression = expression;
        }

        public Multicall Multicall { get; set; }
        public Expression Expression { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is ArrayElementCall)
            {
                return (obj as ArrayElementCall).Expression.Equals(Expression)
                       && (obj as ArrayElementCall).Multicall.Equals(Multicall);
            }
            return false;
        }

    }
    public enum BinaryOperator
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }
    public enum UnaryOperator
    {
        LogicalNegate,
        Negate,
        Identity,
    }
}
