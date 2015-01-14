namespace EvalutionCS.Ast
{
    using System;
    using Sigil.NonGeneric;
    public abstract class Literal
    {
        public abstract void LoadConstant(Emit emitter);

        public abstract Type GetExpressionType();
    }
}