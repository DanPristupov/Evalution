namespace EvalutionCS.Ast
{
    using System;
    using Sigil.NonGeneric;

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

        public override void BuildBody(Emit emitter, Context ctx)
        {
            var leftType = LeftExpression.GetExpressionType(ctx);
            var rightType = RightExpression.GetExpressionType(ctx);

            if (BinaryOperator == BinaryOperator.Add)
            {
                LeftExpression.BuildBody(emitter, ctx);
                RightExpression.BuildBody(emitter, ctx);
                if (IsPrimitiveType(leftType))
                {
                    emitter.Add();
                }
                else
                {
                    var addMethod = leftType.GetMethod("op_Addition", new[] { leftType, rightType });
                    emitter.Call(addMethod);
                }
                return;
            }
            if (BinaryOperator == BinaryOperator.Subtract)
            {
                LeftExpression.BuildBody(emitter, ctx);
                RightExpression.BuildBody(emitter, ctx);
                if (IsPrimitiveType(leftType))
                {
                    emitter.Subtract();
                }
                else
                {
                    var subtractMethod = leftType.GetMethod("op_Subtraction", new[] { leftType, rightType });
                    emitter.Call(subtractMethod);
                }
                return;
            }
            if (BinaryOperator == BinaryOperator.Multiply)
            {
                LeftExpression.BuildBody(emitter, ctx);
                RightExpression.BuildBody(emitter, ctx);
                emitter.Multiply();
                return;
            }
            if (BinaryOperator == BinaryOperator.Divide)
            {
                LeftExpression.BuildBody(emitter, ctx);
                RightExpression.BuildBody(emitter, ctx);
                emitter.Divide();
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
    }
}