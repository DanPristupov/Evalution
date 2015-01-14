namespace EvalutionCS.Ast
{
    using System;
    using Sigil.NonGeneric;

    public class BoolLiteral : Literal
    {
        public bool Value { get; set; }
        public override bool Equals(object obj)
        {
            if (obj is BoolLiteral)
            {
                return (obj as BoolLiteral).Value.Equals(Value);
            }
            return false;
        }

        public override void LoadConstant(Emit emitter)
        {
            throw new System.NotImplementedException();
        }

        public override Type GetExpressionType()
        {
            return typeof (bool);
        }
    }
}