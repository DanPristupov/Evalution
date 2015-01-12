namespace Evalution.CSharpTest
{
    using EvalutionCS.Ast;
    using NUnit.Framework;

    [TestFixture]
    public class AstBuilderTest
    {
        [Test]
        public void TestAddition()
        {
            var result = AstBuilder.Build("1+2");

            var expectedResult =
                new BinaryExpression(
                    new LiteralExpression(new Int32Literal(1)),
                    BinaryOperator.Add,
                    new LiteralExpression(new Int32Literal(2)));

            Assert.AreEqual(expectedResult, result);
        }
    }
}