namespace EvalutionCS.Ast
{
    using System;
    using System.Reflection.Emit;

    public abstract class Expression
    {
        public abstract void BuildBody(ILGenerator emitter, Context ctx);
        public abstract Type GetExpressionType(Context ctx);
    }
}