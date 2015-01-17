namespace Evalution.Ast
{
    using System;
    using System.Reflection.Emit;

    public class ObjectContextPropertyCall : Multicall
    {
        public ObjectContextPropertyCall(Multicall multicall, string identifier)
        {
            Multicall = multicall;
            Identifier = identifier;
        }

        public Multicall Multicall { get; set; }
        public string Identifier { get; set; }

        public override Type BuildBody(ILGenerator il, Context ctx)
        {
            var subPropertyType = Multicall.BuildBody(il, ctx);
            if (subPropertyType.IsValueType && !subPropertyType.IsPrimitive)
            {
                var local = il.DeclareLocal(subPropertyType);
                il.Emit(OpCodes.Stloc, local.LocalIndex);
                il.Emit(OpCodes.Ldloca_S, local.LocalIndex);
            }

            var propertyMethod = ctx.TypeCache.GetTypeProperty(subPropertyType, Identifier).GetGetMethod();
            il.Emit(OpCodes.Callvirt, propertyMethod);

            return propertyMethod.ReturnType;
        }

        public override Type GetExpressionType(Context ctx)
        {
            var subPropertyType = Multicall.GetExpressionType(ctx);
            return ctx.TypeCache.GetTypeProperty(subPropertyType, Identifier).GetGetMethod().ReturnType;
        }

        #region Equals
        public override bool Equals(object obj)
        {
            if (obj is ObjectContextPropertyCall)
            {
                return (obj as ObjectContextPropertyCall).Identifier.Equals(Identifier)
                       && (obj as ObjectContextPropertyCall).Multicall.Equals(Multicall);
            }
            return false;
        }
        #endregion
    }
}