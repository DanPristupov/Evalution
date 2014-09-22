using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Evalution.CSharpTest
{
    [TestClass]
    public class ClassBuilderTest
    {
        [TestMethod]
        public void GeneralTest_NonGeneric()
        {
            var classBuilder = new ClassBuilder(typeof(ClassInt32));
// TODO:
//            classBuilder.Setup(x => x.ValueWithExpression, "2+2*2");
//            classBuilder.Setup(x => x.DependentValue1, "Value1*2");
//            classBuilder.Setup(x => x.DependentValue2, "DependentValue1*2");
            var target = (ClassInt32)classBuilder.BuildObject();

            target.Value1 = 4;
            Assert.AreEqual(6, target.ValueWithExpression); // "2+2*2"
            Assert.AreEqual(8, target.DependentValue1); // "Value1*2"
            Assert.AreEqual(16, target.DependentValue2); // "DependentValue1*2"
        }

        [TestMethod]
        public void GeneralTest_Generic()
        {
            var classBuilder = new ClassBuilder<ClassInt32>()
                .Setup(x => x.ValueWithExpression, "2+2*2")
                .Setup(x => x.DependentValue1, "Value1*2")
                .Setup(x => x.DependentValue2, "DependentValue1*2");

            var target = classBuilder.BuildObject();

            target.Value1 = 4;
            Assert.AreEqual(6, target.ValueWithExpression); // "2+2*2"
            Assert.AreEqual(8, target.DependentValue1); // "Value1*2"
            Assert.AreEqual(16, target.DependentValue2); // "DependentValue1*2"
        }
    }
}