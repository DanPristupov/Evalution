namespace Evalution
{
    using System.Reflection;
    using System.Reflection.Emit;

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