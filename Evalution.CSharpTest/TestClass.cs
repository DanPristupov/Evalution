namespace Evalution.CSharpTest
{
    public class ClassWithDependency
    {
        public int Value1 { get; set; }

        [Expression("2+2*2")]
        public virtual int ValueWithExpression { get; set; }
        // I stopped here. IMplement variables.
//        [Expression("Value1*2")] 
//        public virtual int DependentValue1 { get; set; }
//
//        [Expression("DependentValue1*2")]
//        public virtual int DependentValue2 { get; set; }
    }
}