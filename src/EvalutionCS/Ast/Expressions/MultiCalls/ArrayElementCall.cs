namespace EvalutionCS.Ast
{
    using System;
    using System.Reflection.Emit;

    public class ArrayElementCall : Multicall
    {
        public ArrayElementCall(Multicall multicall, Expression expression)
        {
            Multicall = multicall;
            Expression = expression;
        }

        public Multicall Multicall { get; set; }
        public Expression Expression { get; set; }

        public override Type BuildBody(ILGenerator il, Context ctx)
        {
            var subPropertyType = Multicall.BuildBody(il, ctx);
            Expression.BuildBody(il, ctx);

            var elementType = subPropertyType.GetElementType();
            il.Emit(OpCodes.Ldelem, elementType);
            return elementType;
        }

        public override Type GetExpressionType(Context ctx)
        {
            var subPropertyType = Multicall.GetExpressionType(ctx);
            return subPropertyType.GetElementType();
        }

        #region Equals
        public override bool Equals(object obj)
        {
            if (obj is ArrayElementCall)
            {
                return (obj as ArrayElementCall).Expression.Equals(Expression)
                       && (obj as ArrayElementCall).Multicall.Equals(Multicall);
            }
            return false;
        }
        #endregion
    }
}