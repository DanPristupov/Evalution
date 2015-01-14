namespace EvalutionCS.Ast
{
    using System;
    using System.Reflection;

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

        public override Type BuildBody(BuildArguments args)
        {
            var result = GetDefaultContextProperty(args);
            var target = result.Item1;
            var method = result.Item2;
            if (target == args.TargetType)
            {
                args.Emitter.LoadArgument((UInt16)0);
                args.Emitter.CallVirtual(method);
                return method.ReturnType;
            }
            else
            {
                args.Emitter.Call(method);
                return method.ReturnType;
            }

        }

        public override Type GetExpressionType(BuildArguments args)
        {
            var resultTuple = GetDefaultContextProperty(args);
            return resultTuple.Item2.ReturnType; 
        }

        private Tuple<Type, MethodInfo> GetDefaultContextProperty(BuildArguments args)
        {
            // Priorities: CurrentObject, EnvironmentObject

            foreach (var objectContext in args.ObjectContexts)
            {
                var propertyInfo = args.TypeCache.GetTypeProperty(objectContext, Identifier);
                if (propertyInfo != null)
                {
                    return new Tuple<Type, MethodInfo>(objectContext, propertyInfo.GetGetMethod());
                }
            }
            throw new InvalidNameException(Identifier);

        }
    }
}