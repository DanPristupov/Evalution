namespace EvalutionCS.Ast
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;

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

        public override Type BuildBody(ILGenerator il, Context ctx)
        {
            var result = GetDefaultContextMethod(ctx);
            var method = result.Item2;
            var target = result.Item1;
            if (target == ctx.TargetType)
            {
                
//                emitter.LoadArgument((UInt16)0);
                il.Emit(OpCodes.Ldarg_0);
                foreach (var expression in Arguments)
                {
                    expression.BuildBody(il, ctx);
                }
                il.Emit(OpCodes.Callvirt, method);
//                emitter.CallVirtual(method);
                return method.ReturnType;
            }
            else
            {
                foreach (var expression in Arguments)
                {
                    expression.BuildBody(il, ctx);
                }
                il.Emit(OpCodes.Call, method);
//                emitter.Call(method);
                return method.ReturnType;
            }

        }

        public override Type GetExpressionType(Context ctx)
        {
            var resultTuple = GetDefaultContextMethod(ctx);
            return resultTuple.Item2.ReturnType; 

        }

        private Tuple<Type, MethodInfo> GetDefaultContextMethod(Context ctx)
        {
            // Priorities: CurrentObject, EnvironmentObject

            foreach (var objectContext in ctx.ObjectContexts)
            {
                var methodInfo = ctx.TypeCache.GetTypeMethod(objectContext, Identifier);
                if (methodInfo != null)
                {
                    return new Tuple<Type, MethodInfo>(objectContext, methodInfo);
                }
            }
            throw new InvalidNameException(Identifier);
        }

    }
}