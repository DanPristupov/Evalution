namespace EvalutionCS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class TypeCache
    {
        private Dictionary<Type, PropertyInfo[]> _properties = new Dictionary<Type, PropertyInfo[]>(); 
        private Dictionary<Type, MethodInfo[]> _methods = new Dictionary<Type, MethodInfo[]>();

        public MethodInfo GetTypeMethod(Type type, string methodName)
        {
            return GetMethods(type).FirstOrDefault(x => x.Name == methodName);
        }
        public PropertyInfo GetTypeProperty(Type type, string propertyName)
        {
            return GetProperties(type).FirstOrDefault(x => x.Name == propertyName);
        }
        public MethodInfo GetTypePropertyMethod(Type type, string propertyName)
        {
            return GetProperties(type).FirstOrDefault(x => x.Name == propertyName).GetGetMethod();
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
    }
}