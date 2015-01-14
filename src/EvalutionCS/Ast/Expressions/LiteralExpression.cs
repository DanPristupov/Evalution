namespace EvalutionCS.Ast
{
    using System;

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

        public override void BuildBody(BuildArguments args)
        {
            Literal.LoadConstant(args.Emitter);
        }

        public override Type GetExpressionType(BuildArguments args)
        {
            return Literal.GetExpressionType();
        }
    }
}