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

        public static MethodBuilder DefineVirtualGetMethod(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            return DefineVirtualMethod(typeBuilder, "get_" + propertyName, propertyType);
        }

        public static MethodBuilder DefineVirtualSetMethod(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            return DefineVirtualMethod(typeBuilder, "set_" + propertyName, propertyType);
        }

        public static MethodBuilder DefineVirtualMethod(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            var methodBuilder = typeBuilder.DefineMethod(propertyName,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig |
                MethodAttributes.Virtual, CallingConventions.Standard | CallingConventions.HasThis,
                propertyType, new[] {propertyType});
            return methodBuilder;
        }


        public static PropertyBuilder DefineProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            return typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
        }

        public static void BuildAutoProperty(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder, MethodBuilder getMethodBuilder, MethodBuilder setMethodBuilder)
        {
            // http://www.codeproject.com/Articles/121568/Dynamic-Type-Using-Reflection-Emit#heading0017
            // todo: CreateField
            var field = typeBuilder.DefineField("_" + propertyBuilder.Name, propertyBuilder.PropertyType, FieldAttributes.Private);

            var getIlGen = getMethodBuilder.GetILGenerator();
            getIlGen.Emit(OpCodes.Ldarg_0);
            getIlGen.Emit(OpCodes.Ldfld, field);
            getIlGen.Emit(OpCodes.Ret);

            var setIlGen = setMethodBuilder.GetILGenerator();
            setIlGen.Emit(OpCodes.Ldarg_0);
            setIlGen.Emit(OpCodes.Ldarg_1);
            setIlGen.Emit(OpCodes.Stfld, field);
            setIlGen.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getMethodBuilder);
            propertyBuilder.SetSetMethod(setMethodBuilder);
        }
    }
}