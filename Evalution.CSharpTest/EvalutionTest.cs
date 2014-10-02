using System;
using NUnit.Framework;

namespace Evalution.CSharpTest
{
    [TestFixture]
    public class ClassBuilderTest
    {
        [Test]
        public void GeneralTest_NonGeneric()
        {
            var classBuilder = new ClassBuilder(typeof(ClassInt32))
                .Setup("ValueWithExpression", "2+2*2")
                .Setup("DependentValue1", "Value1*2")
                .Setup("DependentValue2", "DependentValue1*2");
            var target = (ClassInt32)classBuilder.BuildObject();
            
            target.Value1 = 4;
            Assert.AreEqual(6, target.ValueWithExpression); // "2+2*2"
            Assert.AreEqual(8, target.DependentValue1); // "Value1*2"
            Assert.AreEqual(16, target.DependentValue2); // "DependentValue1*2"
        }

        [Test]
        public void GeneralTest_Generic()
        {
            var classBuilder = new ClassBuilder<ClassInt32>()
                .Setup(x => x.ValueWithExpression, "2+2*2")
                .Setup(x => x.DependentValue1, "Value1*2")
                .Setup(x => x.DependentValue2, "DependentValue1*2");
            var target = classBuilder.BuildObject();

            target.Value1 = 4;
            Assert.AreEqual(6, target.ValueWithExpression); // "2+2*2"
            Assert.AreEqual(8, target.DependentValue1); // "Value1*2"
            Assert.AreEqual(16, target.DependentValue2); // "DependentValue1*2"
        }

        [Test]
        public void GeneralTest_Double()
        {
            var classBuilder = new ClassBuilder<ClassDouble>()
                .Setup(x => x.ValueWithExpression,  "2.0+2.0*2.5")
                .Setup(x => x.DependentValue1,      "Value1*2.0")
                .Setup(x => x.DependentValue2,      "DependentValue1*2.0");
            var target = classBuilder.BuildObject();

            target.Value1 = 1.5;
            Assert.AreEqual(7.0, target.ValueWithExpression);   // "2.0 + 2.0 * 2.5"
            Assert.AreEqual(3.0, target.DependentValue1);       // "Value1 * 2.0"
            Assert.AreEqual(6.0, target.DependentValue2);       // "DependentValue1 * 2.0"
        }
        
        [Test]
        public void GeneralTest_DateTime()
        {
            var start = new DateTime(2000, 1, 1);
            var classBuilder = new ClassBuilder<ClassDateTime>()
                .Setup(x => x.ValueWithExpression1, "TimeSpan.FromHours(4) + TimeSpan.FromHours(1)")
                .Setup(x => x.ValueWithExpression2, "new DateTime(2014,1,1) + TimeSpan.FromHours(1)")
                .Setup(x => x.DependentValue1, "Start + Duration")
                .Setup(x => x.DependentValue2, "Start + TimeSpan.FromHours(4)")
                .Setup(x => x.DependentValue3, "End - Start");
            var target = classBuilder.BuildObject();

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
            var classBuilder = new ClassBuilder<ComplexObject>()
                .Setup(x => x.DependentValue1, "Child.Value1*2")
                .Setup(x => x.DependentValue2, "DependentValue1*2");
            var target = classBuilder.BuildObject();
            
            var child = new ClassInt32 { Value1 = 4 };
            target.Child = child;

            Assert.AreEqual(3.0, target.DependentValue1);       // "Child.Value1*2"
            Assert.AreEqual(7.0, target.DependentValue2);       // "DependentValue1*2
        }


    }
}