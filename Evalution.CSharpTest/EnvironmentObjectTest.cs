using System;
using NUnit.Framework;

namespace Evalution.CSharpTest
{
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
        public void MethodTest()
        {
            var classBuilder = new ClassBuilder<TargetClass>()
                .AddEnvironment(typeof (EnvironmentClass))
                .Setup(x => x.Value1, "EnvironmentMethod1()")
//                .Setup(x => x.Value2, "1 + EnvironmentValue")
//                .Setup(x => x.Value3, "TwoHours")
                ;
            
            var target = classBuilder.BuildObject();

            Assert.AreEqual(1212, target.Value1);                   // "EnvironmentValue"
//            Assert.AreEqual(31338, target.Value2);                   // "1 + EnvironmentValue"
//            Assert.AreEqual(TimeSpan.FromHours(2), target.Value3);   // "TwoHours"
        }

        #region TestHelpers

        public class TargetClass
        {
            public virtual int Value1 { get; set; }
            public virtual int Value2 { get; set; }
            public virtual TimeSpan Value3 { get; set; }
        }

        public static class EnvironmentClass
        {
            public static int EnvironmentMethod1()
            {
                return 1212;
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