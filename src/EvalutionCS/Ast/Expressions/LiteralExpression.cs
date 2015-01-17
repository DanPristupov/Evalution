namespace Evalution.Ast
{
    using System;
    using System.Reflection.Emit;

    public class LiteralExpression : Expression
    {
        public LiteralExpression(Literal literal)
        {
            Literal = literal;
        }

        public Literal Literal { get; set; }

        public override void BuildBody(ILGenerator il, Context ctx)
        {
            Literal.LoadConstant(il);
        }

        public override Type GetExpressionType(Context ctx)
        {
            return Literal.GetExpressionType();
        }

        #region Equals
        public override bool Equals(object obj)
        {
            if (obj is LiteralExpression)
            {
                var typedObj = obj as LiteralExpression;
                return typedObj.Literal.Equals(Literal);
            }
            return false;
        }
        #endregion

    }
}