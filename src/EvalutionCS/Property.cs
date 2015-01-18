namespace Evalution
{
    using System.Reflection;
    using System.Reflection.Emit;

    public class Property
    {
        public Property(PropertyDefinition propertyDefinition, PropertyBuilder propertyBuilder, MethodBuilder getMethodBuilder)
            :this(propertyDefinition, propertyBuilder, getMethodBuilder, null)
        {
        }

        public Property(PropertyDefinition propertyDefinition, PropertyBuilder propertyBuilder, MethodBuilder getMethodBuilder, MethodBuilder setMethodBuilder)
        {
            PropertyDefinition = propertyDefinition;
            PropertyBuilder = propertyBuilder;
            GetMethodBuilder = getMethodBuilder;
            SetMethodBuilder = setMethodBuilder;
        }

        public string Name
        {
            get { return PropertyBuilder.Name; }
        }

        public PropertyDefinition PropertyDefinition { get; set; }
        public PropertyBuilder PropertyBuilder { get; private set; }
        public MethodBuilder GetMethodBuilder { get; private set; }
        public MethodBuilder SetMethodBuilder { get; private set; }
    }
}