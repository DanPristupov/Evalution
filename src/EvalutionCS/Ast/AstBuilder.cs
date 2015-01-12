using System;
using System.Collections.Generic;

namespace EvalutionCS.Ast
{
    public class AstBuilder
    {
        public static Program Build(string expression)
        {
            throw new NotImplementedException();
        }
    }

    public class Program
    {
        public Expression Expression { get; set; }
    }

    public abstract class Expression
    {
    }

    public class BinaryExpression : Expression
    {
        public Expression LeftExpression { get; set; }
        public BinaryOperator BinaryOperator { get; set; }
        public Expression RightExpression { get; set; }
    }

    public class LiteralExpression : Expression
    {
        public Literal Literal { get; set; }
    }
    public class UnaryExpression : Expression
    {
        public UnaryOperator UnaryOperator { get; set; }
        public Expression Expression { get; set; }
    }
    public class MultiCallExpression : Expression
    {
        public Multicall Multicall { get; set; }
    }

    public abstract class Literal
    {
        
    }

    public class BoolLiteral : Literal
    {
        public bool Value { get; set; }
    }
    public class Int32Literal : Literal
    {
        public int Value { get; set; }
    }
    public class DoubleLiteral : Literal
    {
        public double Value { get; set; }
    }

    public abstract class Multicall
    {
    }

    public class CurrentContextMethodCall : Multicall
    {
        public string Identifier { get; set; }
    }
    public class CurrentContextPropertyCall : Multicall
    {
        public string Identifier { get; set; }
        public List<Expression> Arguments { get; set; }
    }
    public class ObjectContextPropertyCall : Multicall
    {
        public Multicall Multicall { get; set; }
        public string Identifier { get; set; }
    }
    public class ObjectContextMethodCall : Multicall
    {
        public Multicall Multicall { get; set; }
        public string Identifier { get; set; }
        public List<Expression> Arguments { get; set; }
    }
    public class ArrayElementCall : Multicall
    {
        public Multicall Multicall { get; set; }
        public Expression Expression { get; set; }
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
