using NUnit.Framework;

namespace Evalution.CSharpTest
{
    [TestFixture]
    public class EnvironmentObjectTest
    {
        [Test]
        public void GeneralTest()
        {
            var classBuilder = new ClassBuilder<TargetClass>()
                .AddEnvironment(typeof (EnvironmentClass))
                .Setup(x => x.Value, "1 + EnvironmentValue")
                ;
            
            var target = classBuilder.BuildObject();

            Assert.AreEqual(31338, target.Value);   // "1 + EnvironmentValue"
        }

        #region TestHelpers

        public class TargetClass
        {
            public int Value { get; set; }
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