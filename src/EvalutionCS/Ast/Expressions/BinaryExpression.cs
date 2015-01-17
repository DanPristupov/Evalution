namespace EvalutionCS.Ast
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

            if (BinaryOperator == BinaryOperator.Add)
            {
                LeftExpression.BuildBody(il, ctx);
                RightExpression.BuildBody(il, ctx);
                if (IsPrimitiveType(leftType))
                {
                    il.Emit(OpCodes.Add);
                }
                else
                {
                    var addMethod = leftType.GetMethod("op_Addition", new[] { leftType, rightType });
                    il.Emit(OpCodes.Call, addMethod);
                }
                return;
            }
            if (BinaryOperator == BinaryOperator.Subtract)
            {
                LeftExpression.BuildBody(il, ctx);
                RightExpression.BuildBody(il, ctx);
                if (IsPrimitiveType(leftType))
                {
                    il.Emit(OpCodes.Sub);
                }
                else
                {
                    var subtractMethod = leftType.GetMethod("op_Subtraction", new[] { leftType, rightType });
                    il.Emit(OpCodes.Call, subtractMethod);
                }
                return;
            }
            if (BinaryOperator == BinaryOperator.Multiply)
            {
                LeftExpression.BuildBody(il, ctx);
                RightExpression.BuildBody(il, ctx);
                il.Emit(OpCodes.Mul);
                return;
            }
            if (BinaryOperator == BinaryOperator.Divide)
            {
                LeftExpression.BuildBody(il, ctx);
                RightExpression.BuildBody(il, ctx);
                il.Emit(OpCodes.Div);
                return;
            }
            throw new Exception("Unknown binary operator");
        }

        private bool IsPrimitiveType(Type type)
        {
            if (type == typeof(int) || type == typeof(double))
            {
                return true;
            }
            return false;
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