using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evalution;

namespace Evaluator.CSharpTest
{
    [TestClass]
    public class EvaluatorTest
    {
        [TestMethod]
        public void GeneralTest_Integer()
        {
            var evaluator = new Evalution.Evaluator();
            var target = evaluator.BuildObject<TestClass>();
            Assert.AreEqual(6, target.ValueWithExpression);
        }
    }
}
