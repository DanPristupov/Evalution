namespace EvalutionCS.Ast
{
    using System;
    using System.Reflection.Emit;

    public class BoolLiteral : Literal
    {
        private static Type _type = typeof(bool);

        public bool Value { get; set; }
        public override bool Equals(object obj)
        {
            if (obj is BoolLiteral)
            {
                return (obj as BoolLiteral).Value.Equals(Value);
            }
            return false;
        }

        public override void LoadConstant(ILGenerator il)
        {
            throw new NotImplementedException();
        }

        public override Type GetExpressionType()
        {
            return _type;
        }
    }
}