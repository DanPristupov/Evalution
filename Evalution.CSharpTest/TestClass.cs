using System;

namespace Evalution.CSharpTest
{
    public class ClassInt32
    {
        public int Value1 { get; set; }

        public virtual int ValueWithExpression { get; set; }

        public virtual int DependentValue1 { get; set; }

        public virtual int DependentValue2 { get; set; }
    }

    public class ClassInt32WithAttributes
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

    public class ClassDateTime
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpan Duration { get; set; }

        [Expression("TimeSpan.FromHours(4) + TimeSpan.FromHours(1)")]
        public virtual TimeSpan ValueWithExpression1 { get; set; }

        [Expression("new DateTime(2014,1,1) + TimeSpan.FromHours(1)")]
        public virtual DateTime ValueWithExpression2 { get; set; }

        [Expression("Start + Duration")]
        public virtual DateTime DependentValue1 { get; set; }

        [Expression("Start + TimeSpan.FromHours(4)")]
        public virtual DateTime DependentValue2 { get; set; }

        [Expression("End - Start")]
        public virtual TimeSpan DependentValue3 { get; set; }
    }

    public class ComplexObject
    {
        public ClassInt32 Child { get; set; }

        [Expression("Child.Value1*2")]
        public virtual int DependentValue1 { get; set; }

        [Expression("DependentValue1*2")]
        public virtual int DependentValue2 { get; set; }
    }

}