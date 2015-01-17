using System;
using NUnit.Framework;

namespace Evalution.Tests
{
    using Evalution;

    // todo: make a test that the injection target property is virtual or abstract
    // todo: make a test to check the variable name priorities: current class - environment static class, environment class
    [TestFixture]
    public class EnvironmentObjectTest
    {
        [Test]
        public void PropertyTest()
        {
            var classBuilder = new ClassBuilder<TargetClass>()
                .AddEnvironment(typeof (EnvironmentClass))
                .Setup(x => x.Value1, "EnvironmentValue")
                .Setup(x => x.Value2, "1 + EnvironmentValue")
                .Setup(x => x.Value3, "TwoHours")
                ;
            
            var target = classBuilder.BuildObject();

            Assert.AreEqual(31337, target.Value1);                   // "EnvironmentValue"
            Assert.AreEqual(31338, target.Value2);                   // "1 + EnvironmentValue"
            Assert.AreEqual(TimeSpan.FromHours(2), target.Value3);   // "TwoHours"
        }

        [Test]
        public void MethodTest_NoArguments()
        {
            var classBuilder = new ClassBuilder<TargetClass>()
                .AddEnvironment(typeof (EnvironmentClass))
                .Setup(x => x.Value1, "EnvironmentMethod1()")
                .Setup(x => x.Value2, "1 + EnvironmentMethod1()")
                ;
            
            var target = classBuilder.BuildObject();

            Assert.AreEqual(1212, target.Value1);                   // "EnvironmentMethod1()"
            Assert.AreEqual(1213, target.Value2);                   // "1 + EnvironmentMethod1()"
        }

        [Test]
        public void MethodTest_OneArgument()
        {
            var classBuilder = new ClassBuilder<TargetClass>()
                .AddEnvironment(typeof (EnvironmentClass))
                .Setup(x => x.Value1, "EnvironmentMethod2(1)")
                .Setup(x => x.Value2, "1 + EnvironmentMethod2(1)")
                ;
            
            var target = classBuilder.BuildObject();

            Assert.AreEqual(2, target.Value1);                   // "EnvironmentMethod2(1)"
            Assert.AreEqual(3, target.Value2);                   // "1 + EnvironmentMethod2(1)"
        }

        [Test]
        public void MethodTest_MultipleArguments()
        {
            var classBuilder = new ClassBuilder<TargetClass>()
                .AddEnvironment(typeof (EnvironmentClass))
                .Setup(x => x.Value1, "EnvironmentMethodSum(1, 2)")
                ;
            
            var target = classBuilder.BuildObject();

            Assert.AreEqual(3, target.Value1);                   // "EnvironmentMethod2(1, 2)"
        }

        [Test] // todo: move this test to another class
        public void MethodTest_CallChain()
        {
            var classBuilder = new ClassBuilder<TargetClass>()
                .Setup(x => x.Value1, "1 + SubItem(4).SubItem(4).Value1")
                ;
            
            var target = classBuilder.BuildObject();
            
            Assert.AreEqual(5, target.Value1);                   // "1 + SubItem(4).Value1"
        }

        [Test]
        public void MethodTest_SubCalls()
        {
            var classBuilder = new ClassBuilder<TargetClass>()
                .AddEnvironment(typeof (EnvironmentClass))
                .Setup(x => x.Value1, "EnvironmentMethodSum(EnvironmentMethod1(), EnvironmentMethod2(1))")
                ;
            
            var target = classBuilder.BuildObject();

            Assert.AreEqual(1214, target.Value1); // "EnvironmentMethodSum(EnvironmentMethod1(), EnvironmentMethod2(1))"
        }

        [Test]
        public void MethodTest_VirtualCall()
        {
            var classBuilder = new ClassBuilder<TargetClass>()
                .AddEnvironment(typeof(TimeSpan))
                .Setup(x => x.ValueDouble, "FromHours(3.0).TotalMinutes")
                ;

            var target = classBuilder.BuildObject();

            Assert.AreEqual(180, target.ValueDouble); // "FromHours(3.0).TotalMinutes"
        }

        [Test]
        public void MethodTest_TimeSpan1()
        {
            var classBuilder = new ClassBuilder<TargetClass>()
                .AddEnvironment(typeof (TimeSpan))
                .Setup(x => x.Value3, "FromHours(3.0)")
                ;
            
            var target = classBuilder.BuildObject();

            Assert.AreEqual(TimeSpan.FromHours(3), target.Value3); // "FromHours(3.0)"
        }

        [Test]
        public void MethodTest_TimeSpan2()
        {
            var start = new DateTime(2000, 1, 1);
            var classBuilder = new ClassBuilder<TargetClass>()
                .AddEnvironment(typeof (EnvironmentClass))
                .AddEnvironment(typeof (DateTime))
                .Setup(x => x.Value3, "TwoHours + TwoHours")
                .Setup(x => x.Value4, "Now + TwoHours")
                ;
            
            var target = classBuilder.BuildObject();

            Assert.AreEqual(TimeSpan.FromHours(4), target.Value3); // "FromHours(3.0)"
        }

        #region TestHelpers

        public class TargetClass
        {
            public virtual int Value1 { get; set; }
            public virtual int Value2 { get; set; }
            public virtual double ValueDouble { get; set; }
            public virtual TimeSpan Value3 { get; set; }
            public virtual DateTime Value4 { get; set; }

            public TargetClass SubItem(int val)
            {
                return new TargetClass { Value1 = val };
            }
        }

        public static class EnvironmentClass
        {
            public static int EnvironmentMethod1()
            {
                return 1212;
            }

            public static int EnvironmentMethod2(int arg)
            {
                return arg + 1;
            }

            public static int EnvironmentMethodSum(int arg1, int arg2)
            {
                return arg1 + arg2;
            }

            public static int EnvironmentValue
            {
                get { return 31337; }
            }

            public static TimeSpan TwoHours
            {
                get { return TimeSpan.FromHours(2); }
            }
        }
        #endregion
    }
}