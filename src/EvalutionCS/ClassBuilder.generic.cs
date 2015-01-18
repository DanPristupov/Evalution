namespace Evalution
{
    using System;
    using System.Linq.Expressions;

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
            return base.Setup((property.Body as MemberExpression).Member.Name, expression) as ClassBuilder<T>;
        }
        public ClassBuilder<T> SetupRuntime(string propertyName, Type propertyType, string expression)
        {
            return base.SetupRuntime(propertyName, propertyType, expression) as ClassBuilder<T>;
        }
    }
}
