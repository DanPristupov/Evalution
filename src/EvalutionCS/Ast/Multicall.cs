namespace EvalutionCS.Ast
{
    using System;

    public abstract class Multicall
    {
        public abstract Type BuildBody(BuildArguments args);
        public abstract Type GetExpressionType(BuildArguments args);
    }
}