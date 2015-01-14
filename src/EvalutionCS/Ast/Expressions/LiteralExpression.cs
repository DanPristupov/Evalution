namespace EvalutionCS.Ast
{
    using System;
    using Sigil.NonGeneric;

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

        public override void BuildBody(Emit emitter, Context ctx)
        {
            Literal.LoadConstant(emitter);
        }

        public override Type GetExpressionType(Context ctx)
        {
            return Literal.GetExpressionType();
        }
    }
}