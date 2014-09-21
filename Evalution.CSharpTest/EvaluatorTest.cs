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
            var target = evaluator.BuildObject<ClassInt32>();

            target.Value1 = 4;
            Assert.AreEqual(6, target.ValueWithExpression); // "2+2*2"
            Assert.AreEqual(8, target.DependentValue1);     // "Value1*2"
            Assert.AreEqual(16, target.DependentValue2);    // "DependentValue1*2"
        }

        [TestMethod]
        public void GeneralTest_Double()
        {
            var evaluator = new Evaluator();
            var target = evaluator.BuildObject<ClassDouble>();

            target.Value1 = 1.5;
            Assert.AreEqual(7.0, target.ValueWithExpression);   // "2.0 + 2.0 * 2.5"
            Assert.AreEqual(3.0, target.DependentValue1);       // "Value1 * 2.0"
            Assert.AreEqual(6.0, target.DependentValue2);       // "DependentValue1 * 2.0"
        }
    }
}
