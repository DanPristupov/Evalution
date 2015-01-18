namespace Evalution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using Ast;

    public class ClassBuilder
    {
        private readonly Type _targetType;

        private Type _resultType = null;
        private readonly List<Type> _environmentClasses = new List<Type>();
        private readonly List<PropertyDefinition> _propertyDefinitions = new List<PropertyDefinition>();
        private readonly TypeCache _typeCache = new TypeCache();

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

            var objectProperties = DefineProperties(typeBuilder);

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

        private IEnumerable<Property> DefineProperties(TypeBuilder typeBuilder)
        {
            return _propertyDefinitions
                .Select(propertyDefinition => DefineProperty(typeBuilder, propertyDefinition))
                .ToList();
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

        private static Property DefineProperty(TypeBuilder typeBuilder, PropertyDefinition prop)
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
            var typeBuilder = EmitHelper.CreateTypeBuilder("EV_" + baseType.Name);

            typeBuilder.SetParent(baseType);

            EmitHelper.CreateProxyConstructorsToBase(typeBuilder, baseType);

            return typeBuilder;
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
}
