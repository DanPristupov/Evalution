namespace EvalutionCS.Ast
{
    using System;
    using Sigil.NonGeneric;

    public abstract class Expression
    {
        public abstract void BuildBody(Emit emitter, Context ctx);
        public abstract Type GetExpressionType(Context ctx);
    }
}