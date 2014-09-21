namespace Evalution.CSharpTest
{
    public class ClassInt32
    {
        public int Value1 { get; set; }

        [Expression("2+2*2")]
        public virtual int ValueWithExpression { get; set; }

        [Expression("Value1*2")]
        public virtual int DependentValue1 { get; set; }

        [Expression("DependentValue1*2")]
        public virtual int DependentValue2 { get; set; }
    }

    public class ClassDouble
    {
        public double Value1 { get; set; }

        [Expression("2.0+2.0*2.5")]
        public virtual double ValueWithExpression { get; set; }

        [Expression("Value1*2.0")]
        public virtual double DependentValue1 { get; set; }

        [Expression("DependentValue1*2.0")]
        public virtual double DependentValue2 { get; set; }
    }
}