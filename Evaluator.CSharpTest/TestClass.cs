using System;
using Evalution;

namespace Evaluator.CSharpTest
{
    public class ExpressionAttribute:Attribute
    {
        public ExpressionAttribute(string expression)
        {
            Expression = expression;
        }
        public string Expression { get; set; }
    }

    public class TestClass
    {
        public int Value1 { get; set; }

        [Expression("2+2*2")]
        public virtual int ValueWithExpression { get; set; }

        [Expression("Value1*2")]
        public virtual int DependentValue1 { get; set; }

        [Expression("DependentValue1*2")]
        public virtual int DependentValue2 { get; set; }
    }
}