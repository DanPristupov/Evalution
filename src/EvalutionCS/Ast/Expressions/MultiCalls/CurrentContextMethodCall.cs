namespace EvalutionCS.Ast
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Sigil.NonGeneric;

    public class CurrentContextMethodCall : Multicall
    {
        public CurrentContextMethodCall(string identifier, List<Expression> arguments)
        {
            Identifier = identifier;
            Arguments = arguments;
        }

        public string Identifier { get; set; }
        public List<Expression> Arguments { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is CurrentContextMethodCall)
            {
                var typedObj = obj as CurrentContextMethodCall;
                for (var i = 0; i < Arguments.Count; i++)
                {
                    if (!Arguments[i].Equals(typedObj.Arguments[i]))
                    {
                        return false;
                    }
                }
                return typedObj.Identifier.Equals(Identifier);
            }
            return false;
        }

        public override Type BuildBody(BuildArguments args)
        {
            var result = GetDefaultContextMethod(args);
            var method = result.Item2;
            var target = result.Item1;
            if (target == args.TargetType)
            {
                args.Emitter.LoadArgument((UInt16)0);
                foreach (var expression in Arguments)
                {
                    expression.BuildBody(args);
                }
                args.Emitter.CallVirtual(method);
                return method.ReturnType;
            }
            else
            {
                foreach (var expression in Arguments)
                {
                    expression.BuildBody(args);
                }
                args.Emitter.Call(method);
                return method.ReturnType;
            }

        }

        public override Type GetExpressionType(BuildArguments args)
        {
            var resultTuple = GetDefaultContextMethod(args);
            return resultTuple.Item2.ReturnType; 

        }

        private Tuple<Type, MethodInfo> GetDefaultContextMethod(BuildArguments args)
        {
            // Priorities: CurrentObject, EnvironmentObject

            foreach (var objectContext in args.ObjectContexts)
            {
                var methodInfo = args.TypeCache.GetTypeMethod(objectContext, Identifier);
                if (methodInfo != null)
                {
                    return new Tuple<Type, MethodInfo>(objectContext, methodInfo);
                }
            }
            throw new InvalidNameException(Identifier);
        }

    }
}