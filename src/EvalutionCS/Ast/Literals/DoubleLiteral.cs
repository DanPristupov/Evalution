namespace EvalutionCS.Ast
{
    using System;
    using System.Reflection.Emit;

    public class DoubleLiteral : Literal
    {
        private static Type _type = typeof(Double);

        public DoubleLiteral(double value)
        {
            Value = value;
        }

        public double Value { get; set; }

        public override void LoadConstant(ILGenerator il)
        {
            il.Emit(OpCodes.Ldc_R8, Value);
        }

        public override Type GetExpressionType()
        {
            return _type;
        }

        #region Equals
        public override bool Equals(object obj)
        {
            if (obj is DoubleLiteral)
            {
                return (obj as DoubleLiteral).Value.Equals(Value);
            }
            return false;
        }
        #endregion

    }
}