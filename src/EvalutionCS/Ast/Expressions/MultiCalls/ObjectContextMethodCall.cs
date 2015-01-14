namespace EvalutionCS.Ast
{
    using System;
    using System.Collections.Generic;

    public class ObjectContextMethodCall : Multicall
    {
        public ObjectContextMethodCall(Multicall multicall, string identifier, List<Expression> arguments)
        {
            Multicall = multicall;
            Identifier = identifier;
            Arguments = arguments;
        }

        public Multicall Multicall { get; set; } // rename to PrevCall
        public string Identifier { get; set; }
        public List<Expression> Arguments { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is ObjectContextMethodCall)
            {
                var typedObj = obj as ObjectContextMethodCall;
                for (var i = 0; i < Arguments.Count; i++)
                {
                    if (!Arguments[i].Equals(typedObj.Arguments[i]))
                    {
                        return false;
                    }
                }
                return typedObj.Identifier.Equals(Identifier)
                       && typedObj.Multicall.Equals(Multicall);
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
            foreach (var expression in Arguments)
            {
                expression.BuildBody(args);
            }

            var method = args.TypeCache.GetTypeMethod(subPropertyType, Identifier);

            args.Emitter.CallVirtual(method);
            return method.ReturnType;
        }

        public override Type GetExpressionType(BuildArguments args)
        {
            var subPropertyType = Multicall.GetExpressionType(args);
            return args.TypeCache.GetTypeMethod(subPropertyType, Identifier).ReturnType;
        }
    }
}