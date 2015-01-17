namespace Evalution.Ast
{
    using System;
    using System.Reflection.Emit;

    public class BinaryExpression : Expression
    {
        public BinaryExpression(Expression leftExpression, BinaryOperator binaryOperator, Expression rightExpression)
        {
            LeftExpression = leftExpression;
            BinaryOperator = binaryOperator;
            RightExpression = rightExpression;
        }

        public Expression LeftExpression { get; set; }
        public BinaryOperator BinaryOperator { get; set; }
        public Expression RightExpression { get; set; }

        public override void BuildBody(ILGenerator il, Context ctx)
        {
            var leftType = LeftExpression.GetExpressionType(ctx);
            var rightType = RightExpression.GetExpressionType(ctx);

            LeftExpression.BuildBody(il, ctx);
            RightExpression.BuildBody(il, ctx);

            switch (BinaryOperator)
            {
                case BinaryOperator.Add:
                    if (IsPrimitiveType(leftType))
                    {
                        il.Emit(OpCodes.Add);
                    }
                    else
                    {
                        il.Emit(OpCodes.Call, leftType.GetMethod("op_Addition", new[] { leftType, rightType }));
                    }
                    return;
                case BinaryOperator.Subtract:
                    if (IsPrimitiveType(leftType))
                    {
                        il.Emit(OpCodes.Sub);
                    }
                    else
                    {
                        il.Emit(OpCodes.Call, leftType.GetMethod("op_Subtraction", new[] { leftType, rightType }));
                    }
                    return;
                case BinaryOperator.Multiply:
                    il.Emit(OpCodes.Mul);
                    return;
                case BinaryOperator.Divide:
                    il.Emit(OpCodes.Div);
                    return;
                default:
                    throw new Exception("Unknown binary operator");
            }
        }

        private bool IsPrimitiveType(Type type)
        {
            return type == typeof(int) || type == typeof(double);
        }


        public override Type GetExpressionType(Context ctx)
        {
            return LeftExpression.GetExpressionType(ctx);
        }

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj is BinaryExpression)
            {
                var typedObj = obj as BinaryExpression;
                return typedObj.LeftExpression.Equals(LeftExpression)
                       && typedObj.BinaryOperator.Equals(BinaryOperator)
                       && typedObj.RightExpression.Equals(RightExpression);
            }
            return false;
        }

        #endregion
    }
}