namespace EvalutionCS.Ast
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    public class CurrentContextPropertyCall : Multicall
    {
        public CurrentContextPropertyCall(string identifier)
        {
            Identifier = identifier;
        }

        public string Identifier { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is CurrentContextPropertyCall)
            {
                return (obj as CurrentContextPropertyCall).Identifier.Equals(Identifier);
            }
            return false;
        }

        public override Type BuildBody(ILGenerator il, Context ctx)
        {
            var result = GetDefaultContextProperty(ctx);
            var target = result.Item1;
            var method = result.Item2;
            if (target == ctx.TargetType)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Callvirt, method);
                return method.ReturnType;
            }
            else
            {
                il.Emit(OpCodes.Call, method);
                return method.ReturnType;
            }

        }

        public override Type GetExpressionType(Context ctx)
        {
            var resultTuple = GetDefaultContextProperty(ctx);
            return resultTuple.Item2.ReturnType; 
        }

        private Tuple<Type, MethodInfo> GetDefaultContextProperty(Context ctx)
        {
            // Priorities: CurrentObject, EnvironmentObject

            foreach (var objectContext in ctx.ObjectContexts)
            {
                var propertyInfo = ctx.TypeCache.GetTypeProperty(objectContext, Identifier);
                if (propertyInfo != null)
                {
                    return new Tuple<Type, MethodInfo>(objectContext, propertyInfo.GetGetMethod());
                }
            }
            throw new InvalidNameException(Identifier);

        }
    }
}