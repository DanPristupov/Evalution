namespace Evalution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;
    using Ast;

    public class ClassBuilder
    {
        private readonly Type _targetType;

        private Type _resultType = null;
        private List<Type> _environmentClasses = new List<Type>();
        private List<PropertyDefinition> _propertyDefinitions = new List<PropertyDefinition>();
        private TypeCache _typeCache = new TypeCache();

        public ClassBuilder(Type targetType)
        {
            _targetType = targetType;
        }

        public ClassBuilder AddEnvironment(Type environmentClass)
        {
            CheckResultTypeIsNotBuilt();

            _environmentClasses.Add(environmentClass);
            return this;
        }

        private void CheckResultTypeIsNotBuilt()
        {
            if (_resultType != null)
            {
                throw new InvalidOperationException("Type has been already built and cannot be updated after that.");
            }
        }

        public object BuildObject(params object[] parameters)
        {
            if (_resultType == null)
            {
                _resultType = CreateType();
            }
            return Activator.CreateInstance(_resultType, parameters);
        }

        private Type CreateType()
        {
            var typeBuilder = CreateTypeBuilder(_targetType);

            var objectProperties = new List<Property>();
            foreach (var propertyDefinition in _propertyDefinitions)
            {
                objectProperties.Add(InitializeProperty(typeBuilder, propertyDefinition));
            }

            var ctx = new Context(_typeCache, _targetType, ObjectContexts, objectProperties);

            foreach (var property in ctx.ObjectProperties)
            {
                BuildProperty(property.MethodInfo as MethodBuilder,
                    property.PropertyInfo as PropertyBuilder,
                    property.PropertyDefinition,
                    ctx);
            }
            return typeBuilder.CreateType();
        }

        private void BuildProperty(MethodBuilder methodBuilder, PropertyBuilder propertyBuilder, PropertyDefinition prop, Context ctx)
        {
            var ilGen = methodBuilder.GetILGenerator();

            var expression = AstBuilder.Build(prop.Expression);
            expression.BuildBody(ilGen, ctx);
            ilGen.Emit(OpCodes.Ret);
            var getMethodBuilder = methodBuilder;
            propertyBuilder.SetGetMethod(getMethodBuilder);
        }

        private static Property InitializeProperty(TypeBuilder typeBuilder, PropertyDefinition prop)
        {
            var propertyBuilder = typeBuilder.DefineProperty(prop.PropertyName,
                PropertyAttributes.HasDefault, prop.PropertyType, null);

            var methodBuilder = typeBuilder.DefineMethod("get_" + prop.PropertyName,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig |
                MethodAttributes.Virtual, CallingConventions.Standard | CallingConventions.HasThis,
                prop.PropertyType, new Type[0]);
            return new Property(prop, propertyBuilder, methodBuilder);
        }

        private IEnumerable<Type> ObjectContexts
        {
            get
            {
                yield return _targetType;
                foreach (var environmentClass in _environmentClasses)
                {
                    yield return environmentClass;
                }
            }
        } 

        private TypeBuilder CreateTypeBuilder(Type baseType)
        {
            // todo: should assemblyName be the same for all classes?
            var assemblyName = new AssemblyName("EV_" + baseType.Name); // may be I should use assembly of the objType?
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("EvalutionModule");
            var typeBuilder = moduleBuilder.DefineType("EV" + baseType.Name,
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout,
                null);

            typeBuilder.SetParent(baseType);

            CreateProxyConstructors(typeBuilder, baseType);

            return typeBuilder;
        }

        private void CreateProxyConstructors(TypeBuilder typeBuilder, Type baseType)
        {
            foreach (var constructor in baseType.GetConstructors())
            {
                CreateProxyConstructor(typeBuilder, constructor);
            }
        }

        private void CreateProxyConstructor(TypeBuilder typeBuilder, ConstructorInfo ctor)
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

        public ClassBuilder Setup(string property, string expression)
        {
            CheckResultTypeIsNotBuilt();
            var propertyInfo = _typeCache.GetTypeProperty(_targetType, property);

            _propertyDefinitions.Add(new PropertyDefinition(propertyInfo.Name, propertyInfo.PropertyType, expression));
            return this;
        }

        public ClassBuilder SetupRuntime(string propertyName, Type propertyType, string expression)
        {
            _propertyDefinitions.Add(new PropertyDefinition(propertyName, propertyType, expression));
            return this;
        }
    }

    public class ClassBuilder<T> : ClassBuilder where T : class
    {
        public ClassBuilder()
            : base(typeof (T))
        {
        }

        public ClassBuilder<T> AddEnvironment(Type environmentClass)
        {
            return base.AddEnvironment(environmentClass) as ClassBuilder<T>;
        }

        public T BuildObject(params object[] parameters)
        {
            return base.BuildObject(parameters) as T;
        }

        public ClassBuilder<T> Setup<TProperty>(Expression<Func<T, TProperty>> property, string expression)
        {
            return base.Setup((property.Body as System.Linq.Expressions.MemberExpression).Member.Name, expression) as ClassBuilder<T>;
        }
    }

    public class Property
    {
        public Property(PropertyDefinition propertyDefinition, PropertyBuilder propertyBuilder, MethodBuilder methodBuilder)
        {
            PropertyDefinition = propertyDefinition;
            PropertyInfo = propertyBuilder;
            MethodInfo = methodBuilder;
        }

        public string Name
        {
            get { return PropertyInfo.Name; }
        }

        public PropertyDefinition PropertyDefinition { get; set; }
        public PropertyInfo PropertyInfo { get; private set; }
        public MethodInfo MethodInfo { get; private set; }
    }
}
