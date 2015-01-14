namespace EvalutionCS.Ast
{
    using System;
    using Sigil.NonGeneric;

    public class Int32Literal : Literal
    {
        public Int32Literal(int value)
        {
            Value = value;
        }

        public int Value { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj is Int32Literal)
            {
                return (obj as Int32Literal).Value.Equals(Value);
            }
            return false;
        }

        public override void LoadConstant(Emit emitter)
        {
            emitter.LoadConstant(Value);
        }

        public override Type GetExpressionType()
        {
            return typeof (int);
        }
    }
}