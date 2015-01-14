namespace EvalutionCS.Ast
{
    using System;
    using Sigil.NonGeneric;

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

        public override void BuildBody(Emit emitter, Context ctx)
        {
            Multicall.BuildBody(emitter, ctx);
        }

        public override Type GetExpressionType(Context ctx)
        {
            return Multicall.GetExpressionType(ctx);
        }
    }
}