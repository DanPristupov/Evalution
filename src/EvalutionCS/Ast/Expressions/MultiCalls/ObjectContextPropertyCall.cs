namespace EvalutionCS.Ast
{
    using System;

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

        public override Type BuildBody(BuildArguments args)
        {
            var subPropertyType = Multicall.BuildBody(args);
            if (subPropertyType.IsValueType && !subPropertyType.IsPrimitive)
            {
                args.Emitter.DeclareLocal(subPropertyType, "value1");
                args.Emitter.StoreLocal("value1");
                args.Emitter.LoadLocalAddress("value1");
            }
            var propertyMethod = args.TypeCache.GetTypePropertyMethod(subPropertyType, Identifier);
            args.Emitter.CallVirtual(propertyMethod);
            return propertyMethod.ReturnType;
        }

        public override Type GetExpressionType(BuildArguments args)
        {
            var subPropertyType = Multicall.GetExpressionType(args);
            return args.TypeCache.GetTypeProperty(subPropertyType, Identifier).GetGetMethod().ReturnType;
        }
    }
}