namespace EvalutionCS.Ast
{
    using System;
    using Sigil.NonGeneric;

    public class ArrayElementCall : Multicall
    {
        public ArrayElementCall(Multicall multicall, Expression expression)
        {
            Multicall = multicall;
            Expression = expression;
        }

        public Multicall Multicall { get; set; }
        public Expression Expression { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is ArrayElementCall)
            {
                return (obj as ArrayElementCall).Expression.Equals(Expression)
                       && (obj as ArrayElementCall).Multicall.Equals(Multicall);
            }
            return false;
        }

        public override Type BuildBody(Emit emitter, Context ctx)
        {
            var subPropertyType = Multicall.BuildBody(emitter, ctx);
            Expression.BuildBody(emitter, ctx);

            var elementType = subPropertyType.GetElementType();
            emitter.LoadElement(elementType);
            return elementType;

        }

        public override Type GetExpressionType(Context ctx)
        {
            var subPropertyType = Multicall.GetExpressionType(ctx);
            return subPropertyType.GetElementType();

        }
    }
}