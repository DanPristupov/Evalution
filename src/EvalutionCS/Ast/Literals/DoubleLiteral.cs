namespace EvalutionCS.Ast
{
    using System;
    using System.Reflection.Emit;

    public class DoubleLiteral : Literal
    {
        public DoubleLiteral(double value)
        {
            Value = value;
        }

        public double Value { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is DoubleLiteral)
            {
                return (obj as DoubleLiteral).Value.Equals(Value);
            }
            return false;
        }

        public override void LoadConstant(ILGenerator il)
        {
//            emitter.LoadConstant(Value);
            il.Emit(OpCodes.Ldc_R8, Value);
        }

        public override Type GetExpressionType()
        {
            return typeof (double);
        }
    }
}