namespace EvalutionCS.Ast
{
    using System;
    using Sigil.NonGeneric;

    public abstract class Multicall
    {
        public abstract Type BuildBody(Emit emitter, Context ctx);
        public abstract Type GetExpressionType(Context ctx);
    }
}