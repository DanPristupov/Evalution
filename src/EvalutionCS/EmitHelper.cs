namespace Evalution
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    public static class EmitHelper
    {
        public static TypeBuilder CreateTypeBuilder(string name)
        {
            // todo: should assemblyName be the same for all classes?
            var assemblyName = new AssemblyName(name); // may be I should use assembly of the objType?
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("EvalutionModule");
            var typeBuilder = moduleBuilder.DefineType(name,
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout,
                null);
            return typeBuilder;
        }

        public static void CreateProxyConstructorsToBase(TypeBuilder typeBuilder, Type baseType)
        {
            foreach (var constructor in baseType.GetConstructors())
            {
                CreateProxyConstructor(typeBuilder, constructor);
            }
        }

        private static void CreateProxyConstructor(TypeBuilder typeBuilder, ConstructorInfo ctor)
        {
            var parameters = ctor.GetParameters();
            var paramTypes = parameters.Select(x => x.ParameterType).ToArray();

            var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, paramTypes);
            var ilGen = ctorBuilder.GetILGenerator();
            ilGen.Emit(OpCodes.Ldarg_0);

            for (var i = 0; i < parameters.Length; i++)
            {
                ilGen.Emit(OpCodes.Ldarg, (UInt16)(i + 1));
            }
            ilGen.Emit(OpCodes.Call, ctor);
            ilGen.Emit(OpCodes.Ret);
        }

    }
}