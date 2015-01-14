namespace EvalutionCS
{
    using System.Reflection;

    public class PropertyDefinition
    {
        public PropertyDefinition(PropertyInfo propertyInfo, string expression)
        {
            PropertyInfo = propertyInfo;
            Expression = expression;
        }

        public PropertyInfo PropertyInfo { get; private set; }
        public string Expression { get; private set; }
    }
}