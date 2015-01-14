namespace EvalutionCS.Ast
{
    using System;

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

        public override Type BuildBody(BuildArguments args)
        {
            var subPropertyType = Multicall.BuildBody(args);
            Expression.BuildBody(args);

            var elementType = subPropertyType.GetElementType();
            args.Emitter.LoadElement(elementType);
            return elementType;

        }

        public override Type GetExpressionType(BuildArguments args)
        {
            var subPropertyType = Multicall.GetExpressionType(args);
            return subPropertyType.GetElementType();

        }
    }
}