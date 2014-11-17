using NUnit.Framework;

namespace Evalution.CSharpTest
{
    // todo: make a test that the injection target property is virtual or abstract
    // todo: make a test to check the variable name priorities: current class - environment static class, environment class
    [TestFixture]
    public class EnvironmentObjectTest
    {
        [Test]
        public void GeneralTest()
        {
            var classBuilder = new ClassBuilder<TargetClass>()
                .AddEnvironment(typeof (EnvironmentClass))
                .Setup(x => x.Value, "EnvironmentValue")
//                .Setup(x => x.Value, "1 + EnvironmentValue")
                ;
            
            var target = classBuilder.BuildObject();

            Assert.AreEqual(31337, target.Value);   // "1 + EnvironmentValue"
        }

        #region TestHelpers

        public class TargetClass
        {
            public virtual int Value { get; set; }
        }

        public static class EnvironmentClass
        {
            public static int EnvironmentValue
            {
                get { return 31337; }
            }
        }
        #endregion
    }
}