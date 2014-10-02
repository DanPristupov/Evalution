using System;
using System.Linq;
using NUnit.Framework;

namespace Evalution.CSharpTest
{
    [TestFixture]
    public class EvaluatorTest
    {
/*
        [Test]
        public void GeneralTest_Integer()
        {
            var evaluator = new Evaluator();
            var target = evaluator.BuildObject<ClassInt32WithAttributes>();

            target.Value1 = 4;
            Assert.AreEqual(6, target.ValueWithExpression); // "2+2*2"
            Assert.AreEqual(8, target.DependentValue1);     // "Value1*2"
            Assert.AreEqual(16, target.DependentValue2);    // "DependentValue1*2"
        }

        [Test]
        public void GeneralTest_Double()
        {
            var evaluator = new Evaluator();
            var target = evaluator.BuildObject<ClassDouble>();

            target.Value1 = 1.5;
            Assert.AreEqual(7.0, target.ValueWithExpression);   // "2.0 + 2.0 * 2.5"
            Assert.AreEqual(3.0, target.DependentValue1);       // "Value1 * 2.0"
            Assert.AreEqual(6.0, target.DependentValue2);       // "DependentValue1 * 2.0"
        }

        [Test]
        public void GeneralTest_DateTime()
        {
            var start = new DateTime(2000, 1, 1);
            var evaluator = new Evaluator();
            var target = evaluator.BuildObject<ClassDateTime>();

            target.Start = start;
            target.End = start.AddDays(10);
            Assert.AreEqual(3.0, target.ValueWithExpression1);  // "TimeSpan.FromHours(4) + TimeSpan.FromHours(1)"
            Assert.AreEqual(3.0, target.ValueWithExpression2);  // "new DateTime(2014,1,1) + TimeSpan.FromHours(1)"
            Assert.AreEqual(3.0, target.DependentValue1);       // "Start + Duration"
            Assert.AreEqual(7.0, target.DependentValue2);       // "Start + TimeSpan.FromHours(4)"
            Assert.AreEqual(7.0, target.DependentValue3);       // "End - Start"
        }

        [Test]
        public void GeneralTest_ComplexObject()
        {
            var evaluator = new Evaluator();
            var target = evaluator.BuildObject<ComplexObject>();
            var child = new ClassInt32 {Value1 = 4};
            target.Child = child;
            Assert.AreEqual(8, target.DependentValue1);     // "Child.Value1*2"
            Assert.AreEqual(16, target.DependentValue2);    // "DependentValue1*2"
        }
 */
    }
}
