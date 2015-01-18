using System;
using NUnit.Framework;

namespace Evalution.Tests
{
    using Evalution;

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
        public void GeneralTest_RuntimeProperties()
        {
            var classBuilder = new ClassBuilder(typeof(object))
                .SetupRuntime("ValueWithExpression", typeof(int), "2+2*2")
                .SetupRuntime("DependentValue2", typeof(int), "ValueWithExpression*2")
                ;
            var target = (object)classBuilder.BuildObject();
            var type = target.GetType();
            var valueWithExpressionValue = type.GetProperty("ValueWithExpression").GetValue(target, null);
            var dependentValue2Value = type.GetProperty("DependentValue2").GetValue(target, null);
            Assert.AreEqual(6, valueWithExpressionValue);
            Assert.AreEqual(12, dependentValue2Value);
        }

        [Test]
        public void GeneralTest_RuntimeAutoProperties()
        {
            var classBuilder = new ClassBuilder(typeof(object))
                .SetupRuntime("AutoProperty", typeof (double))
                ;
            var target = (object)classBuilder.BuildObject();
            var type = target.GetType();

            type.GetProperty("AutoProperty").SetValue(target, 12.0, null);
            var result = type.GetProperty("AutoProperty").GetValue(target, null);
            Assert.AreEqual(12.0, result);
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
        public void GeneralTest_UnaryOperators()
        {
            var classBuilder = new ClassBuilder<ClassInt32>()
                .Setup(x => x.ValueWithExpression, "-Value1")
                .Setup(x => x.DependentValue1, "-(2+2)+2")
                .Setup(x => x.DependentValue2, "(-2+2)+2")
                .Setup(x => x.DependentValue3, "+(2+2)+2")
                .Setup(x => x.DependentValue4, "(+2+2)+2");
            var target = classBuilder.BuildObject();
            target.Value1 = 1;
            Assert.AreEqual(-1, target.ValueWithExpression);
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
        public void GeneralTest_DoubleAndIntConversions()
        {
            var classBuilder = new ClassBuilder<ClassDouble>()
                .Setup(x => x.ValueWithExpression, "2 + 2.0")
                ;
            
            var target = classBuilder.BuildObject();
            Assert.Fail("TODO");
        }

        [Test]
        public void GeneralTest_TimeSpan()
        {
            var classBuilder = new ClassBuilder<ClassDateTime>()
                .AddEnvironment(typeof(TimeSpan))
                .Setup(x => x.ValueWithExpression1, "FromHours(1.0 + 4.5 * 2.0)")
                .Setup(x => x.ValueWithExpression2, "FromHours(4.5) + FromHours(2.5)")
                .Setup(x => x.DependentValue3,      "ValueWithExpression2 - FromHours(3.0)")
                ;
            var target = classBuilder.BuildObject();

            Assert.AreEqual(TimeSpan.FromHours(10.0), target.ValueWithExpression1); // "TimeSpan.FromHours(1 + 4.5 * 2)"
            Assert.AreEqual(TimeSpan.FromHours(7.0), target.ValueWithExpression2);  // "TimeSpan.FromHours(4.5) + TimeSpan.FromHours(2.5)"
            Assert.AreEqual(TimeSpan.FromHours(4.0), target.DependentValue3);       // "ValueWithExpression2 - TimeSpan.FromHours(3)"
        }

        [Test]
        public void GeneralTest_TimeSpanDateTime()
        {
            var start = new DateTime(2000, 1, 1);
            var classBuilder = new ClassBuilder<ClassDateTime>()
                .AddEnvironment(typeof(TimeSpan))
                .Setup(x => x.ValueWithExpression1, "FromHours(4.0) + FromHours(1.0)")
                .Setup(x => x.DependentValue1,      "Start + Duration")
                .Setup(x => x.DependentValue2,      "Start + FromHours(4.0)")
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

        [Test]
        public void GeneralTest_Arrays()
        {
            var start = new DateTime(2000, 1, 1);

            var classBuilder = new ClassBuilder<ClassArray>()
                .AddEnvironment(typeof (TimeSpan))
                .Setup(x => x.DependentValue1, "IntValues[2] * 2")
                .Setup(x => x.DependentValue2, "IntValues[1] + IntValues[2]")
                .Setup(x => x.DependentValue3, "ComplexObjects[1].Duration + FromHours(2.0)")
                .Setup(x => x.DependentValue4, "ComplexObjects[1].Start + ComplexObjects[0].Duration")
                ;

            var target = classBuilder.BuildObject();
            target.IntValues = new[] {1, 2, 3};
            target.ComplexObjects = new[]
            {
                new ClassDateTime() {Duration = TimeSpan.FromHours(4)},
                new ClassDateTime() {Duration = TimeSpan.FromHours(5), Start = start},
            };

            Assert.AreEqual(6, target.DependentValue1);                         // "IntValues[2] * 2"
            Assert.AreEqual(5, target.DependentValue2);                         // "IntValues[1] + IntValues[2]"
            Assert.AreEqual(TimeSpan.FromHours(7.0), target.DependentValue3);   // "ComplexObjects[1].Duration + TimeSpan.FromHours(2.0)"
            Assert.AreEqual(start.AddHours(4), target.DependentValue4);         // "ComplexObjects[1].Start + ComplexObjects[0].Duration"
        }

        [Test]
        public void MethodCallTest_ValueType()
        {
            var start = new DateTime(2000, 1, 1);
            var classBuilder = new ClassBuilder<ClassDateTime>()
                .Setup(x => x.DependentValue1, "Start.AddHours(2.0)")
                ;

            var target = classBuilder.BuildObject();
            target.Start = start;
            Assert.AreEqual(start.AddHours(2.0), target.DependentValue1); // "Start.AddHours(2.0)"
        }

        [Test]
        public void MethodCallTest_ReferenceType()
        {
            var classBuilder = new ClassBuilder<ClassInt32>()
                .Setup(x => x.DependentValue1, "Multiply(3,4)")
                ;

            var target = classBuilder.BuildObject();
            Assert.AreEqual(12, target.DependentValue1);    // "Multiply(3,4)"
        }

        [Test]
        public void MethodCallTest_ComplexObject()
        {
            var classBuilder = new ClassBuilder<ComplexObject>()
                .Setup(x => x.DependentValue1, "ComplexChild.Multiply(3,5)");
            var target = classBuilder.BuildObject();

            target.ComplexChild = new ComplexObject
            {
                Child = new ClassInt32()
            };

            Assert.AreEqual(15, target.DependentValue1);   // "ComplexChild.Multiply(3,5)"
        }


        [Test]
        public void GeneralTest_Dictionaries()
        {
            Assert.Fail("TODO");
        }

        [Test]
        public void GeneralTest_ConstructorTest()
        {
            var classBuilder = new ClassBuilder<ClassWithConstructor>();
            
            var target1 = classBuilder.BuildObject();
            Assert.True(target1.DefaultContstructorCalled);
            Assert.False(target1.IntConstructurCalled);
            Assert.False(target1.DoubleIntConstructurCalled);
            
            var target2 = classBuilder.BuildObject(1);
            Assert.False(target2.DefaultContstructorCalled);
            Assert.True(target2.IntConstructurCalled);
            Assert.False(target2.DoubleIntConstructurCalled);
            
            var target3 = classBuilder.BuildObject(1, 2);
            Assert.False(target3.DefaultContstructorCalled);
            Assert.False(target3.IntConstructurCalled);
            Assert.True(target3.DoubleIntConstructurCalled);
        }

        [Test]
        [ExpectedException(typeof(InvalidNameException))]
        public void InvalidNameExceptionTest()
        {
            var classBuilder = new ClassBuilder<ClassInt32>()
                .Setup(x => x.ValueWithExpression, "NonExistingProperty");
            classBuilder.BuildObject();
        }

    }
}