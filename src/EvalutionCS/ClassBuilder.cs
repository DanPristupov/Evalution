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

            var objectProperties = DefineProperties(typeBuilder).ToArray();

            var ctx = new Context(_typeCache, _targetType, ObjectContexts, objectProperties);

            foreach (var property in ctx.ObjectProperties)
            {
                BuildProperty(typeBuilder,
                    property.GetMethodBuilder as MethodBuilder,
                    property.PropertyBuilder as PropertyBuilder,
                    property.PropertyDefinition,
                    property,
                    ctx);
            }
            return typeBuilder.CreateType();
        }

        private IEnumerable<Property> DefineProperties(TypeBuilder typeBuilder)
        {
            foreach (var propertyDefinition in _propertyDefinitions)
            {
                var property = DefineProperty(typeBuilder, propertyDefinition);
                yield return property;
            }
        }

        private void BuildProperty(TypeBuilder typeBuilder, MethodBuilder methodBuilder, PropertyBuilder propertyBuilder, PropertyDefinition prop, Property property, Context ctx)
        {

            if (!string.IsNullOrEmpty(prop.Expression))
            {
                var ilGen = property.GetMethodBuilder.GetILGenerator();
                var expression = AstBuilder.Build(prop.Expression);
                expression.BuildBody(ilGen, ctx);
                ilGen.Emit(OpCodes.Ret);
                property.PropertyBuilder.SetGetMethod(property.GetMethodBuilder);
            }
            else
            {
                EmitHelper.BuildAutoProperty(typeBuilder,
                    property.PropertyBuilder,
                    property.GetMethodBuilder,
                    property.SetMethodBuilder);
            }
        }

        private static Property DefineProperty(TypeBuilder typeBuilder, PropertyDefinition prop)
        {
            var propertyBuilder = EmitHelper.DefineProperty(typeBuilder, prop.PropertyName, prop.PropertyType);

            if (!string.IsNullOrEmpty(prop.Expression))
            {
                // todo: why virtual? I don't want it to be virtual
                var getMethodBuilder = EmitHelper.DefineVirtualGetMethod(typeBuilder, prop.PropertyName, prop.PropertyType);
                return new Property(prop, propertyBuilder, getMethodBuilder);
            }
            else
            {
                // todo: remove virtual
                var getMethodBuilder = EmitHelper.DefineVirtualGetMethod(typeBuilder, prop.PropertyName, prop.PropertyType);
                var setMethodBuilder = EmitHelper.DefineVirtualSetMethod(typeBuilder, prop.PropertyName, prop.PropertyType);
                return new Property(prop, propertyBuilder, getMethodBuilder, setMethodBuilder);
            }
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

        // todo: rename to OverrideProperty?
        public ClassBuilder Setup(string property, string expression)
        {
            CheckResultTypeIsNotBuilt();
            var propertyInfo = _typeCache.GetTypeProperty(_targetType, property);

            _propertyDefinitions.Add(new PropertyDefinition(propertyInfo.Name, propertyInfo.PropertyType, expression));
            return this;
        }

        // todo: rename to DefineProperty?
        public ClassBuilder SetupRuntime(string propertyName, Type propertyType, string expression)
        {
            _propertyDefinitions.Add(new PropertyDefinition(propertyName, propertyType, expression));
            return this;
        }

        // todo: rename to DefineProperty?
        public ClassBuilder SetupRuntime(string propertyName, Type propertyType)
        {
            _propertyDefinitions.Add(new PropertyDefinition(propertyName, propertyType));
            return this;
        }
    }
}
