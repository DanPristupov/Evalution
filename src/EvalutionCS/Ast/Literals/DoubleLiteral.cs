namespace EvalutionCS.Ast
{
    using System;
    using Sigil.NonGeneric;

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

        public override void LoadConstant(Emit emitter)
        {
            emitter.LoadConstant(Value);
        }

        public override Type GetExpressionType()
        {
            return typeof (double);
        }
    }
}