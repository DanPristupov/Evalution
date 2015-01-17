namespace EvalutionCS.Ast
{
    using System;
    using System.Collections.Generic;
    using System.Reflection.Emit;

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

        public override Type BuildBody(ILGenerator il, Context ctx)
        {
            var subPropertyType = Multicall.BuildBody(il, ctx);
            if (subPropertyType.IsValueType && !subPropertyType.IsPrimitive)
            {
                var local = il.DeclareLocal(subPropertyType);
                il.Emit(OpCodes.Stloc, local.LocalIndex);
                il.Emit(OpCodes.Ldloca_S, local.LocalIndex);

//                emitter.DeclareLocal(subPropertyType, "value1");
//                emitter.StoreLocal("value1");
//                emitter.LoadLocalAddress("value1");
            }
            foreach (var expression in Arguments)
            {
                expression.BuildBody(il, ctx);
            }

            var method = ctx.TypeCache.GetTypeMethod(subPropertyType, Identifier);

//            emitter.CallVirtual(method);
            il.Emit(OpCodes.Callvirt, method);

            return method.ReturnType;
        }

        public override Type GetExpressionType(Context ctx)
        {
            var subPropertyType = Multicall.GetExpressionType(ctx);
            return ctx.TypeCache.GetTypeMethod(subPropertyType, Identifier).ReturnType;
        }
    }
}