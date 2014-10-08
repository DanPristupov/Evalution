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
                .Setup("ValueWithExpression",   "2+2*2")
                .Setup("DependentValue1",       "Value1*2")
                .Setup("DependentValue2",       "DependentValue1*2");
            var target = (ClassInt32)classBuilder.BuildObject();
            
            target.Value1 = 4;
            Assert.AreEqual(6, target.ValueWithExpression); // "2+2*2"
            Assert.AreEqual(8, target.DependentValue1);     // "Value1*2"
            Assert.AreEqual(16, target.DependentValue2);    // "DependentValue1*2"
        }

        [Test]
        public void GeneralTest_Generic()
        {
            var classBuilder = new ClassBuilder<ClassInt32>()
                .Setup(x => x.ValueWithExpression,  "2+2*2")
                .Setup(x => x.DependentValue1,      "Value1*2")
                .Setup(x => x.DependentValue2,      "DependentValue1*2");
            var target = classBuilder.BuildObject();

            target.Value1 = 4;
            Assert.AreEqual(6, target.ValueWithExpression); // "2+2*2"
            Assert.AreEqual(8, target.DependentValue1);     // "Value1*2"
            Assert.AreEqual(16, target.DependentValue2);    // "DependentValue1*2"
        }

        [Test]
        public void GeneralTest_BinaryExpressions()
        {
            var classBuilder = new ClassBuilder<ClassInt32>()
                .Setup(x => x.DependentValue1, "2+2+2")
                .Setup(x => x.DependentValue2, "2+2-2")
                .Setup(x => x.DependentValue3, "2+2*2")
                .Setup(x => x.DependentValue4, "2+2/2");
            var target = classBuilder.BuildObject();

            Assert.AreEqual(6, target.DependentValue1);
            Assert.AreEqual(2, target.DependentValue2);
            Assert.AreEqual(6, target.DependentValue3);
            Assert.AreEqual(3, target.DependentValue4);
        }

        [Test]
        public void GeneralTest_ParenthesesExpressions()
        {
            var classBuilder = new ClassBuilder<ClassInt32>()
                .Setup(x => x.DependentValue1, "(2+2)+2")
                .Setup(x => x.DependentValue2, "(2+2)-2")
                .Setup(x => x.DependentValue3, "(2+2)*2")
                .Setup(x => x.DependentValue4, "(2+2)/2");
            var target = classBuilder.BuildObject();

            Assert.AreEqual(6, target.DependentValue1);
            Assert.AreEqual(2, target.DependentValue2);
            Assert.AreEqual(8, target.DependentValue3);
            Assert.AreEqual(2, target.DependentValue4);
        }

        [Test]
        public void GeneralTest_UnarySigns()
        {
            var classBuilder = new ClassBuilder<ClassInt32>()
                .Setup(x => x.DependentValue1, "-(2+2)+2")
                .Setup(x => x.DependentValue2, "(-2+2)+2")
                .Setup(x => x.DependentValue3, "+(2+2)+2")
                .Setup(x => x.DependentValue4, "(+2+2)+2");
            var target = classBuilder.BuildObject();

            Assert.AreEqual(-2, target.DependentValue1);
            Assert.AreEqual(2, target.DependentValue2);
            Assert.AreEqual(6, target.DependentValue3);
            Assert.AreEqual(6, target.DependentValue4);
        }

        [Test]
        public void GeneralTest_Double()
        {
            var classBuilder = new ClassBuilder<ClassDouble>()
                .Setup(x => x.ValueWithExpression,  "2.0 + 2.0 * 2.5")
                .Setup(x => x.DependentValue1,      "Value1 * 2.0")
                .Setup(x => x.DependentValue2,      "DependentValue1 * 2.0");
            var target = classBuilder.BuildObject();

            target.Value1 = 1.5;
            Assert.AreEqual(7.0, target.ValueWithExpression);   // "2.0 + 2.0 * 2.5"
            Assert.AreEqual(3.0, target.DependentValue1);       // "Value1 * 2.0"
            Assert.AreEqual(6.0, target.DependentValue2);       // "DependentValue1 * 2.0"
        }

        [Test]
        public void GeneralTest_TimeSpan()
        {
            var classBuilder = new ClassBuilder<ClassDateTime>()
                .Setup(x => x.ValueWithExpression1, "TimeSpan.FromHours(4.5)")
                .Setup(x => x.ValueWithExpression2, "TimeSpan.FromHours(4.5) + TimeSpan.FromHours(2.5)")
                .Setup(x => x.DependentValue3,      "ValueWithExpression2 - TimeSpan.FromHours(3.0)")
                ;
            var target = classBuilder.BuildObject();

            Assert.AreEqual(TimeSpan.FromHours(4.5), target.ValueWithExpression1);  // "TimeSpan.FromHours(4.5)"
            Assert.AreEqual(TimeSpan.FromHours(7), target.ValueWithExpression2);    // "TimeSpan.FromHours(4.5) + TimeSpan.FromHours(2.5)"
            Assert.AreEqual(TimeSpan.FromHours(4.0), target.DependentValue3);       // "ValueWithExpression2 - TimeSpan.FromHours(3)"
        }

        [Test]
        public void GeneralTest_TimeSpanDateTime()
        {
            var start = new DateTime(2000, 1, 1);
            var classBuilder = new ClassBuilder<ClassDateTime>()
                .Setup(x => x.ValueWithExpression1, "TimeSpan.FromHours(4.0) + TimeSpan.FromHours(1.0)")
                .Setup(x => x.DependentValue1,      "Start + Duration")
                .Setup(x => x.DependentValue2,      "Start + TimeSpan.FromHours(4.0)")
                .Setup(x => x.DependentValue3,      "End - Start");
            var target = classBuilder.BuildObject();

            target.Start = start;
            target.End = start.AddHours(10);
            target.Duration = TimeSpan.FromDays(2);
            Assert.AreEqual(TimeSpan.FromHours(5.0),
                target.ValueWithExpression1);  // "TimeSpan.FromHours(4) + TimeSpan.FromHours(1)"
            Assert.AreEqual(new DateTime(2000, 1, 3),
                target.DependentValue1);       // "Start + Duration"
            Assert.AreEqual(new DateTime(2000, 1, 1, 4 , 0 , 0),
                target.DependentValue2);       // "Start + TimeSpan.FromHours(4)"
            Assert.AreEqual(TimeSpan.FromHours(10),
                target.DependentValue3);       // "End - Start"
        }

        [Test]
        public void GeneralTest_ComplexObjectDouble()
        {
            var classBuilder = new ClassBuilder<ComplexObject>()
                .Setup(x => x.DependentValue1, "Child.Value1*2")
                .Setup(x => x.DependentValue2, "DependentValue1*2");
            var target = classBuilder.BuildObject();
            
            var child = new ClassInt32 { Value1 = 3 };
            target.Child = child;

            Assert.AreEqual(6.0, target.DependentValue1);   // "Child.Value1*2"
            Assert.AreEqual(12.0, target.DependentValue2);  // "DependentValue1*2
        }

        [Test]
        public void GeneralTest_ComplexObjectTriple()
        {
            var classBuilder = new ClassBuilder<ComplexObject>()
                .Setup(x => x.DependentValue1,  "ComplexChild.Child.Value1*2");
            var target = classBuilder.BuildObject();
            
            target.ComplexChild = new ComplexObject
            {
                Child = new ClassInt32 { Value1 = 3 }
            };

            Assert.AreEqual(6.0, target.DependentValue1);   // "ComplexChild.Child.Value1*2"
        }

        [Test]
        public void GeneralTest_ComplexEvalutionObject()
        {
            var classBuilder = new ClassBuilder<ComplexObject>()
                .Setup(x => x.DependentValue1,  "Child.DependentValue1*2");
            var target = classBuilder.BuildObject();

            var dependentClassBuilder = new ClassBuilder<ClassInt32>()
                .Setup(x => x.DependentValue1,  "7");
            var dependentObject = dependentClassBuilder.BuildObject();

            target.Child = dependentObject;

            Assert.AreEqual(14.0, target.DependentValue1);   // "Child.DependentValue1*2"
        }


    }
}