namespace EvalutionCS.Ast
{
    using System;

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

        public override void BuildBody(BuildArguments args)
        {
            Multicall.BuildBody(args);
        }

        public override Type GetExpressionType(BuildArguments args)
        {
            return Multicall.GetExpressionType(args);
        }
    }
}