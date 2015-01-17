namespace Evalution.Ast
{
    using System;
    using System.Reflection.Emit;

    public abstract class Multicall
    {
        public abstract Type BuildBody(ILGenerator emitter, Context ctx);
        public abstract Type GetExpressionType(Context ctx);
    }
}