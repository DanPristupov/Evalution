namespace EvalutionCS.Ast
{
    using System;
    using Sigil.NonGeneric;

    public class ObjectContextPropertyCall : Multicall
    {
        public ObjectContextPropertyCall(Multicall multicall, string identifier)
        {
            Multicall = multicall;
            Identifier = identifier;
        }

        public Multicall Multicall { get; set; }
        public string Identifier { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is ObjectContextPropertyCall)
            {
                return (obj as ObjectContextPropertyCall).Identifier.Equals(Identifier)
                       && (obj as ObjectContextPropertyCall).Multicall.Equals(Multicall);
            }
            return false;
        }

        public override Type BuildBody(Emit emitter, Context ctx)
        {
            var subPropertyType = Multicall.BuildBody(emitter, ctx);
            if (subPropertyType.IsValueType && !subPropertyType.IsPrimitive)
            {
                emitter.DeclareLocal(subPropertyType, "value1");
                emitter.StoreLocal("value1");
                emitter.LoadLocalAddress("value1");
            }
            var propertyMethod = ctx.TypeCache.GetTypePropertyMethod(subPropertyType, Identifier);
            emitter.CallVirtual(propertyMethod);
            return propertyMethod.ReturnType;
        }

        public override Type GetExpressionType(Context ctx)
        {
            var subPropertyType = Multicall.GetExpressionType(ctx);
            return ctx.TypeCache.GetTypeProperty(subPropertyType, Identifier).GetGetMethod().ReturnType;
        }
    }
}