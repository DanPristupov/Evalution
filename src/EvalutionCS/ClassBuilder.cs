namespace EvalutionCS
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

    public class ClassBuilder
    {
        private readonly Type _targetType;

        private Type _resultType = null;
        private List<Type> _environmentClasses = new List<Type>();
        private Dictionary<Type, PropertyInfo[]>  _properties = new Dictionary<Type, PropertyInfo[]>();
        private Dictionary<Type, MethodInfo[]> _methods = new Dictionary<Type, MethodInfo[]>();
        private List<PropertyDefinition> _propertyDefinitions = new List<PropertyDefinition>();

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

            GenerateMethodBody(AstBuilder.Build(propertyDefinition.Expression), emitter);
            emitter.Return();
            return emitter.CreateMethod();
        }

        private void GenerateMethodBody(Expression expression, Emit emitter)
        {
            if (expression is BinaryExpression)
            {
                var binaryExpression = expression as BinaryExpression;
                var leftType = GetExpressionType(binaryExpression.LeftExpression);
                var rightType = GetExpressionType(binaryExpression.RightExpression);

                if (binaryExpression.BinaryOperator == BinaryOperator.Add)
                {
                    GenerateMethodBody(binaryExpression.LeftExpression, emitter);
                    GenerateMethodBody(binaryExpression.RightExpression, emitter);
                    if (IsPrimitiveType(leftType))
                    {
                        emitter.Add();
                    }
                    else
                    {
                        var addMethod = leftType.GetMethod("op_Addition", new[] {leftType, rightType});
                        emitter.Call(addMethod);
                    }
                    return;
                }
                if (binaryExpression.BinaryOperator == BinaryOperator.Subtract)
                {
                    GenerateMethodBody(binaryExpression.LeftExpression, emitter);
                    GenerateMethodBody(binaryExpression.RightExpression, emitter);
                    if (IsPrimitiveType(leftType))
                    {
                        emitter.Subtract();
                    }
                    else
                    {
                        var subtractMethod = leftType.GetMethod("op_Subtraction", new[] { leftType, rightType });
                        emitter.Call(subtractMethod);
                    }
                    return;
                }
                if (binaryExpression.BinaryOperator == BinaryOperator.Multiply)
                {
                    GenerateMethodBody(binaryExpression.LeftExpression, emitter);
                    GenerateMethodBody(binaryExpression.RightExpression, emitter);
                    emitter.Multiply();
                    return;
                }
                if (binaryExpression.BinaryOperator == BinaryOperator.Divide)
                {
                    GenerateMethodBody(binaryExpression.LeftExpression, emitter);
                    GenerateMethodBody(binaryExpression.RightExpression, emitter);
                    emitter.Divide();
                    return;
                }
                throw new Exception("Unknown binary operator");
            }

            if (expression is LiteralExpression)
            {
                var literalExpression = expression as LiteralExpression;
                if (literalExpression.Literal is Int32Literal)
                {
                    emitter.LoadConstant((literalExpression.Literal as Int32Literal).Value);
                    return;
                }
                if (literalExpression.Literal is DoubleLiteral)
                {
                    emitter.LoadConstant((literalExpression.Literal as DoubleLiteral).Value);
                    return;
                }
                throw new Exception("Unknown literal");
            }
            if (expression is UnaryExpression)
            {
                var unaryExpression = expression as UnaryExpression;
                if (unaryExpression.UnaryOperator == UnaryOperator.Negate)
                {
                    GenerateMethodBody(unaryExpression.Expression, emitter);
                    emitter.Negate();
                    return;
                }
                if (unaryExpression.UnaryOperator == UnaryOperator.Identity)
                {
                    // We do not need to do anything here.
                    GenerateMethodBody(unaryExpression.Expression, emitter);
                    return;
                }
                if (unaryExpression.UnaryOperator == UnaryOperator.LogicalNegate)
                {
                    throw new NotImplementedException("Logical negate is not implemented yet.");
                }
                throw new Exception("Unknown literal");
            }
            if (expression is MultiCallExpression)
            {
                var multiCallExpression = expression as MultiCallExpression;
                GenerateMulticallBody(multiCallExpression.Multicall, emitter);
                return;
            }
            throw new Exception("Unknown syntax tree element.");
        }

        private Type GenerateMulticallBody(Multicall multicall, Emit emitter)
        {
            if (multicall is CurrentContextMethodCall)
            {
                var currentContextMethodCall = multicall as CurrentContextMethodCall;
                var result = GetDefaultContextMethod(currentContextMethodCall.Identifier);
                var target = result.Item1;
                if (target == _targetType)
                {
                    emitter.LoadArgument((UInt16) 0);
                    foreach (var expression in currentContextMethodCall.Arguments)
                    {
                        GenerateMethodBody(expression, emitter);
                    }
                    return CreateNonStaticMethodCall(result.Item2, emitter);
                }
                else
                {
                    foreach (var expression in currentContextMethodCall.Arguments)
                    {
                        GenerateMethodBody(expression, emitter);
                    }
                    return CreateStaticMethodCall(result.Item2, emitter);
                }
            }
            if (multicall is CurrentContextPropertyCall)
            {
                var currentContextPropertyCall = multicall as CurrentContextPropertyCall;
                var result = GetDefaultContextProperty(currentContextPropertyCall.Identifier);
                var target = result.Item1;
                if (target == _targetType)
                {
                    emitter.LoadArgument((UInt16) 0);
                    return CreateNonStaticMethodCall(result.Item2, emitter);
                }
                else
                {
                    return CreateStaticMethodCall(result.Item2, emitter);
                }
            }
            if (multicall is ObjectContextMethodCall)
            {
                var objectContextMethodCall = multicall as ObjectContextMethodCall;
                var subPropertyType = GenerateMulticallBody(objectContextMethodCall.Multicall, emitter);
                if (subPropertyType.IsValueType && !subPropertyType.IsPrimitive)
                {
                    emitter.DeclareLocal(subPropertyType, "value1");
                    emitter.StoreLocal("value1");
                    emitter.LoadLocalAddress("value1");
                }
                foreach (var expression in objectContextMethodCall.Arguments)
                {
                    GenerateMethodBody(expression, emitter);
                }

                return CreateNonStaticMethodCall(GetTypeMethod(subPropertyType, objectContextMethodCall.Identifier), emitter);
            }
            if (multicall is ObjectContextPropertyCall)
            {
                var objectContextPropertyCall = multicall as ObjectContextPropertyCall;
                var subPropertyType = GenerateMulticallBody(objectContextPropertyCall.Multicall, emitter);
                if (subPropertyType.IsValueType && !subPropertyType.IsPrimitive)
                {
                    emitter.DeclareLocal(subPropertyType, "value1");
                    emitter.StoreLocal("value1");
                    emitter.LoadLocalAddress("value1");
                }

                return CreateNonStaticMethodCall(GetTypePropertyMethod(subPropertyType, objectContextPropertyCall.Identifier), emitter);
            }
            if (multicall is ArrayElementCall)
            {
                var arrayElementCall = multicall as ArrayElementCall;
                var subPropertyType = GenerateMulticallBody(arrayElementCall.Multicall, emitter);
                GenerateMethodBody(arrayElementCall.Expression, emitter);
                var elementType = subPropertyType.GetElementType();
                emitter.LoadElement(elementType);
                return elementType;
            }
            throw new Exception("Unknown multicall expression");
        }

        private MethodInfo GetTypeMethod(Type type, string methodName)
        {
            return GetMethods(type).First(x => x.Name == methodName);
        }
        private MethodInfo GetTypePropertyMethod(Type type, string propertyName)
        {
            return GetProperties(type).First(x => x.Name == propertyName).GetGetMethod();
        }

        private Type CreateNonStaticMethodCall(MethodInfo method, Emit emitter)
        {
            emitter.CallVirtual(method);
            return method.ReturnType;
        }
        private Type CreateStaticMethodCall(MethodInfo method, Emit emitter)
        {
            emitter.Call(method);
            return method.ReturnType;
        }

        private bool IsPrimitiveType(Type t)
        {
            if (t == typeof (int) || t == typeof (double))
            {
                return true;
            }
            return false;
        }
        private Type GetExpressionType(Expression expression)
        {
            if (expression is BinaryExpression)
            {
                var binaryExpression = expression as BinaryExpression;
                return GetExpressionType(binaryExpression.LeftExpression);
            }
            else if (expression is LiteralExpression)
            {
                var literalExpression = expression as LiteralExpression;
                if (literalExpression.Literal is BoolLiteral) return typeof (bool);
                if (literalExpression.Literal is Int32Literal) return typeof(int);
                if (literalExpression.Literal is DoubleLiteral) return typeof(double);
                throw new Exception();
            }
            else if (expression is UnaryExpression)
            {
                return GetExpressionType((expression as UnaryExpression).Expression);
            }
            else if (expression is MultiCallExpression)
            {
                return GetMultiCallExpressionType((expression as MultiCallExpression).Multicall);
            }
            throw new Exception("Unknown expression");
        }

        private Type GetMultiCallExpressionType(Multicall multicall)
        {
            if (multicall is CurrentContextMethodCall)
            {
                var currentContextMethodCall = multicall as CurrentContextMethodCall;
                var resultTuple = GetDefaultContextMethod(currentContextMethodCall.Identifier);
                return resultTuple.Item2.ReturnType; 
            }
            if (multicall is CurrentContextPropertyCall)
            {
                var currentContextPropertyCall = multicall as CurrentContextPropertyCall;
                var resultTuple = GetDefaultContextProperty(currentContextPropertyCall.Identifier);
                return resultTuple.Item2.ReturnType; 
            }
            if (multicall is ObjectContextMethodCall)
            {
                var objectContextMethodCall = multicall as ObjectContextMethodCall;
                var subPropertyType = GetMultiCallExpressionType(objectContextMethodCall.Multicall);
                return GetMethodType(GetMethods(subPropertyType), objectContextMethodCall.Identifier);
            }
            if (multicall is ObjectContextPropertyCall)
            {
                var objectContextPropertyCall = multicall as ObjectContextPropertyCall;
                var subPropertyType = GetMultiCallExpressionType(objectContextPropertyCall.Multicall);
                return GetPropertyType(GetProperties(subPropertyType), objectContextPropertyCall.Identifier);
            }
            if (multicall is ArrayElementCall)
            {
                var arrayElementCall = multicall as ArrayElementCall;
                var subPropertyType = GetMultiCallExpressionType(arrayElementCall.Multicall);
                return subPropertyType.GetElementType();
            }
            throw new Exception("Unknown multicall expression");
        }

        private Type GetMethodType(MethodInfo[] methods, string methodName)
        {
            return methods.First(x => x.Name == methodName).ReturnType;
        }
        private Type GetPropertyType(PropertyInfo[] properties, string methodName)
        {
            return properties.First(x => x.Name == methodName).PropertyType;
        }

        // todo: why it return MethodInfo?
        private Tuple<object, MethodInfo> GetDefaultContextProperty(string propertyName)
        {
            // Priorities: CurrentObject, EnvironmentObject

            foreach (var objectContext in ObjectContexts)
            {
                var propertyInfo = FindProperty(objectContext, propertyName);
                if (propertyInfo != null)
                {
                    return new Tuple<object, MethodInfo>(objectContext, propertyInfo.GetGetMethod());
                }
            }
            throw new InvalidNameException(propertyName);

        }

        private Tuple<Type, MethodInfo> GetDefaultContextMethod(string methodName)
        {
            // Priorities: CurrentObject, EnvironmentObject

            foreach (var objectContext in ObjectContexts)
            {
                var methodInfo = FindMethod(objectContext, methodName);
                if (methodInfo != null)
                {
                    return new Tuple<Type, MethodInfo>(objectContext, methodInfo);
                }
            }
            throw new InvalidNameException(methodName);
        }

        private MethodInfo FindMethod(Type type, string methodName)
        {
            var methods = GetMethods(type);
            var method = methods.FirstOrDefault(x => x.Name == methodName);
            return method;
        }
        private PropertyInfo FindProperty(Type type, string propertyName)
        {
            var methods = GetProperties(type);
            var method = methods.FirstOrDefault(x => x.Name == propertyName);
            return method;
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
            var propertyInfo = GetProperties(_targetType).First(x => x.Name == property);

            _propertyDefinitions.Add(new PropertyDefinition(propertyInfo, expression));
            return this;
        }

        private PropertyInfo[] GetProperties(Type type)
        {
            if (_properties.ContainsKey(type))
            {
                return _properties[type];
            }
            var properties = type.GetProperties();
            _properties[type] = properties;
            return properties;
        }

        private MethodInfo[] GetMethods(Type type)
        {
            if (_methods.ContainsKey(type))
            {
                return _methods[type];
            }
            var methods = type.GetMethods();
            _methods[type] = methods;
            return methods;
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
