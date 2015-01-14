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

        public override void BuildBody(BuildArguments args)
        {
            var leftType = LeftExpression.GetExpressionType(args);
            var rightType = RightExpression.GetExpressionType(args);

            if (BinaryOperator == BinaryOperator.Add)
            {
                LeftExpression.BuildBody(args);
                RightExpression.BuildBody(args);
                if (IsPrimitiveType(leftType))
                {
                    args.Emitter.Add();
                }
                else
                {
                    var addMethod = leftType.GetMethod("op_Addition", new[] { leftType, rightType });
                    args.Emitter.Call(addMethod);
                }
                return;
            }
            if (BinaryOperator == BinaryOperator.Subtract)
            {
                LeftExpression.BuildBody(args);
                RightExpression.BuildBody(args);
                if (IsPrimitiveType(leftType))
                {
                    args.Emitter.Subtract();
                }
                else
                {
                    var subtractMethod = leftType.GetMethod("op_Subtraction", new[] { leftType, rightType });
                    args.Emitter.Call(subtractMethod);
                }
                return;
            }
            if (BinaryOperator == BinaryOperator.Multiply)
            {
                LeftExpression.BuildBody(args);
                RightExpression.BuildBody(args);
                args.Emitter.Multiply();
                return;
            }
            if (BinaryOperator == BinaryOperator.Divide)
            {
                LeftExpression.BuildBody(args);
                RightExpression.BuildBody(args);
                args.Emitter.Divide();
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


        public override Type GetExpressionType(BuildArguments args)
        {
            return LeftExpression.GetExpressionType(args);
        }
    }
}