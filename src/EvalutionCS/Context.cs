namespace Evalution
{
    using System;
    using System.Collections.Generic;

    public class Context
    {
        public TypeCache TypeCache { get; private set; }
        public Type TargetType { get; private set; }
        public IEnumerable<Type> ObjectContexts { get; set; }
        public IEnumerable<Property> ObjectProperties { get; set; }

        public Context(TypeCache typeCache, Type targetType, IEnumerable<Type> objectContexts, IEnumerable<Property> objectProperties)
        {
            TypeCache = typeCache;
            TargetType = targetType;
            ObjectContexts = objectContexts;
            ObjectProperties = objectProperties;
        }
    }
}