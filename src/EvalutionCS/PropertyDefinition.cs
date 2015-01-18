namespace Evalution
{
    using System;

    public class PropertyDefinition
    {
        public PropertyDefinition(string propertyName, Type propertyType, string expression)
        {
            PropertyName = propertyName;
            PropertyType = propertyType;
            Expression = expression;
        }

        public PropertyDefinition(string propertyName, Type propertyType)
            :this(propertyName, propertyType, null)
        {
        }

        public string PropertyName { get; set; }
        public Type PropertyType { get; set; }

        public string Expression { get; private set; }
    }
}