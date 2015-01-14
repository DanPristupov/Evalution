﻿namespace EvalutionCS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;
    using Ast;
    using Sigil.NonGeneric;
    using BinaryExpression = Ast.BinaryExpression;
    using Expression = Ast.Expression;
    using UnaryExpression = Ast.UnaryExpression;

    public class BuildArguments
    {
        public Emit Emitter { get; private set; }
        public TypeCache TypeCache { get; private set; }
        public Type TargetType { get; private set; }
        public IEnumerable<Type> ObjectContexts { get; set; }

        public BuildArguments(Emit emitter, TypeCache typeCache, Type targetType, IEnumerable<Type> objectContexts)
        {
            Emitter = emitter;
            TypeCache = typeCache;
            TargetType = targetType;
            ObjectContexts = objectContexts;
        }
    }
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

            foreach (var propertyDefinition in _propertyDefinitions)
            {
                BuildProperty(typeBuilder, propertyDefinition);
            }
            return typeBuilder.CreateType();
        }

        private void BuildProperty(TypeBuilder typeBuilder, PropertyDefinition propertyDefinition)
        {
            var propertyBuilder = typeBuilder.DefineProperty(propertyDefinition.PropertyInfo.Name,
                PropertyAttributes.HasDefault, propertyDefinition.PropertyInfo.PropertyType, null);
            var getMethodBuilder = CreateGetPropertyMethodBuilder(typeBuilder, propertyDefinition);
            propertyBuilder.SetGetMethod(getMethodBuilder);
        }

        private MethodBuilder CreateGetPropertyMethodBuilder(TypeBuilder typeBuilder, PropertyDefinition propertyDefinition)
        {
            var propertyName = propertyDefinition.PropertyInfo.Name;
            var emitter = Emit.BuildMethod(propertyDefinition.PropertyInfo.PropertyType, new Type[0], typeBuilder,
                "get_" + propertyName,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig |
                MethodAttributes.Virtual,
                CallingConventions.Standard | CallingConventions.HasThis);

            var expression = AstBuilder.Build(propertyDefinition.Expression);
            expression.BuildBody(new BuildArguments(emitter, _typeCache, _targetType, ObjectContexts));
            emitter.Return();
            return emitter.CreateMethod();
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
            var emit = Emit.BuildConstructor(paramTypes, typeBuilder, MethodAttributes.Public, CallingConventions.HasThis);
            emit.LoadArgument((UInt16) 0);
            for (var i = 0; i < parameters.Length; i++)
            {
                emit.LoadArgument((UInt16) (i + 1));
            }
            emit.Call(ctor);
            emit.Return();
            emit.CreateConstructor();
        }

        public ClassBuilder Setup(string property, string expression)
        {
            CheckResultTypeIsNotBuilt();
            var propertyInfo = _typeCache.GetTypeProperty(_targetType, property);

            _propertyDefinitions.Add(new PropertyDefinition(propertyInfo, expression));
            return this;
        }

        public ClassBuilder SetupRuntime(string property, Type propertyType, string expression)
        {
            throw new NotImplementedException();
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
}
