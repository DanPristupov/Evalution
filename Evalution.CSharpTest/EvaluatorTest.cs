using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Evalution.CSharpTest
{
    [TestClass]
    public class EvaluatorTest
    {
        [TestMethod]
        public void GeneralTest_Integer()
        {
            var evaluator = new Evaluator();
            var target = evaluator.BuildObject<ClassWithDependency>();

            target.Value1 = 4;
            Assert.AreEqual(6, target.ValueWithExpression); // "2+2*2"
            Assert.AreEqual(8, target.DependentValue1);     // "Value1*2"
            Assert.AreEqual(16, target.DependentValue2);    // "DependentValue1*2"
        }
    }
}
