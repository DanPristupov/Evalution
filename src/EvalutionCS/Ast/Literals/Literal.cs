namespace EvalutionCS.Ast
{
    using System;
    using System.Reflection.Emit;
    public abstract class Literal
    {
        public abstract void LoadConstant(ILGenerator emitter);

        public abstract Type GetExpressionType();
    }
}