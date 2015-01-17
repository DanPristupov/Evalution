namespace EvalutionCS.Ast
{
    using System;
    using System.Reflection.Emit;

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

        public override void BuildBody(ILGenerator il, Context ctx)
        {
            Multicall.BuildBody(il, ctx);
        }

        public override Type GetExpressionType(Context ctx)
        {
            return Multicall.GetExpressionType(ctx);
        }
    }
}