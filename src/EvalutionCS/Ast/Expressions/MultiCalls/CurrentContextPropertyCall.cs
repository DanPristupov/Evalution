namespace EvalutionCS.Ast
{
    using System;
    using System.Reflection;
    using Sigil.NonGeneric;

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

        public override Type BuildBody(Emit emitter, Context ctx)
        {
            var result = GetDefaultContextProperty(ctx);
            var target = result.Item1;
            var method = result.Item2;
            if (target == ctx.TargetType)
            {
                emitter.LoadArgument((UInt16)0);
                emitter.CallVirtual(method);
                return method.ReturnType;
            }
            else
            {
                emitter.Call(method);
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