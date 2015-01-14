namespace EvalutionCS.Ast
{
    using System;
    using Sigil.NonGeneric;

    public abstract class Expression
    {
        public abstract void BuildBody(BuildArguments args);
        public abstract Type GetExpressionType(BuildArguments args);
    }
}