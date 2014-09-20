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
            Assert.AreEqual(6, target.ValueWithExpression);
        }
    }
}
