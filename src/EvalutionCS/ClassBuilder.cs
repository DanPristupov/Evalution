namespace EvalutionCS
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public class ClassBuilder
    {
        private readonly Type _targetType;

        private Type _resultType = null;
        private List<Type> _environmentClasses = new List<Type>(); 

        public ClassBuilder(Type targetType)
        {
            _targetType = targetType;
        }

        public ClassBuilder AddEnvironment(Type environmentClass)
        {
            if (_resultType != null)
            {
                throw new InvalidOperationException("Type has been already built and cannot be updated after that.");
            }
            _environmentClasses.Add(environmentClass);
        }

        public object BuildObject(params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public ClassBuilder Setup(string property, string expression)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public ClassBuilder<T> AddEnvironment(Type environmentClass)
        {
            throw new NotImplementedException();
        }

        public T BuildObject(params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public ClassBuilder<T> Setup<TProperty>(Expression<Func<T, TProperty>> property, string expression)
        {
            throw new NotImplementedException();
        }
    }
}
