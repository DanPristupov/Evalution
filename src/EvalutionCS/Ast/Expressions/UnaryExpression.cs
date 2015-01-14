namespace EvalutionCS.Ast
{
    using System;
    using Sigil.NonGeneric;

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

        public override void BuildBody(Emit emitter, Context ctx)
        {
            if (UnaryOperator == UnaryOperator.Negate)
            {
                Expression.BuildBody(emitter, ctx);
                emitter.Negate();
                return;
            }
            if (UnaryOperator == UnaryOperator.Identity)
            {
                // We do not need to do anything here.
                Expression.BuildBody(emitter, ctx);
                return;
            }
            if (UnaryOperator == UnaryOperator.LogicalNegate)
            {
                throw new NotImplementedException("Logical negate is not implemented yet.");
            }

        }

        public override Type GetExpressionType(Context ctx)
        {
            return Expression.GetExpressionType(ctx);
        }
    }
}