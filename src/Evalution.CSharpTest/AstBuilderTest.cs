namespace Evalution.CSharpTest
{
    using System.Collections.Generic;
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

            Assert.AreEqual(expectedResult, result);;
        }

        [Test]
        public void TestSubtract()
        {
            var result = AstBuilder.Build("1-2");

            var expectedResult =
                new BinaryExpression(
                    new LiteralExpression(new Int32Literal(1)),
                    BinaryOperator.Subtract,
                    new LiteralExpression(new Int32Literal(2)));

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TestMultiply()
        {
            var result = AstBuilder.Build("1*2");


            var expectedResult =
                new BinaryExpression(
                    new LiteralExpression(new Int32Literal(1)),
                    BinaryOperator.Multiply,
                    new LiteralExpression(new Int32Literal(2)));

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TestDivide()
        {
            var result = AstBuilder.Build("1/2");


            var expectedResult =
                new BinaryExpression(
                    new LiteralExpression(new Int32Literal(1)),
                    BinaryOperator.Divide,
                    new LiteralExpression(new Int32Literal(2)));

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TestInt32Literal()
        {
            var result = AstBuilder.Build("1");


            var expectedResult =
                new LiteralExpression(new Int32Literal(1));

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TestDoubleLiteral()
        {
            var result = AstBuilder.Build("1.5");


            var expectedResult =
                new LiteralExpression(new DoubleLiteral(1.5));

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TestBinaryExpressionInt32DivideDouble()
        {
            var result = AstBuilder.Build("3/2");


            var expectedResult =
                new BinaryExpression(
                    new LiteralExpression(new Int32Literal(3)),
                    BinaryOperator.Divide,
                    new LiteralExpression(new Int32Literal(2)));

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TestSpaces()
        {
            var result = AstBuilder.Build("  1.5 +	2 ");


            var expectedResult =
                new BinaryExpression(
                    new LiteralExpression(new DoubleLiteral(1.5)),
                    BinaryOperator.Add,
                    new LiteralExpression(new Int32Literal(2)));

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TestParentheses()
        {
            var result = AstBuilder.Build("( 1.5 + 1 ) * 2");

            var expectedResult =
                new BinaryExpression(
                    new BinaryExpression(
                        new LiteralExpression(new DoubleLiteral(1.5)),
                        BinaryOperator.Add,
                        new LiteralExpression(new Int32Literal(1))),
                    BinaryOperator.Multiply,
                    new LiteralExpression(new Int32Literal(2)));

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TestMethodCallExpression(){
            var result = AstBuilder.Build("method(1.0,2+3)");

            var expectedResult =
                new MultiCallExpression(
                    new CurrentContextMethodCall(
                        "method",
                        new List<Expression>()
                        {
                            new LiteralExpression(
                                new DoubleLiteral(1.0)
                                ),
                            new BinaryExpression(
                                new LiteralExpression(new Int32Literal(2)),
                                BinaryOperator.Add,
                                new LiteralExpression(new Int32Literal(3))
                                )
                        }
                        )
                    );

            Assert.AreEqual(expectedResult, result);
        }
        [Test]
        public void TestMethodChainCallExpression1(){
            var result = AstBuilder.Build("Method1(1).Method2(2)"); 

            var expectedResult =
                new MultiCallExpression(
                    new ObjectContextMethodCall(
                        new CurrentContextMethodCall(
                            "Method1", new List<Expression>(){new LiteralExpression(new Int32Literal(1))}
                        ), "Method2", new List<Expression>{new LiteralExpression(new Int32Literal(2))}
                    )
                );

            Assert.AreEqual(expectedResult, result);
        }
        [Test]
        public void TestMethodChainCallExpression2(){
            var result = AstBuilder.Build("Method1(1).Prop1.Method2(2)"); 

            var expectedResult =
                new MultiCallExpression(
                    new ObjectContextMethodCall(
                        new ObjectContextPropertyCall(
                            new CurrentContextMethodCall(
                                "Method1", new List<Expression>{new LiteralExpression(new Int32Literal(1))}
                            ),
                            "Prop1" ),
                        "Method2", new List<Expression> { new LiteralExpression(new Int32Literal(2)) }
                    )
                );

            Assert.AreEqual(expectedResult, result);
        }
        [Test]
        public void TestMethodCallExpression_NoArguments(){
            var result = AstBuilder.Build("method()");

            var expectedResult =
                new MultiCallExpression(
                    new CurrentContextMethodCall(
                        "method",
                        new List<Expression>()
                        )
                    );

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TestMultiCallExpression_Single()
        {
            var result = AstBuilder.Build("var1");


            var expectedResult =
                new MultiCallExpression(
                    new CurrentContextPropertyCall("var1"));

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TestMultiCallExpression_Multiple()
        {
            var result = AstBuilder.Build("var1.var2.var3");


            var expectedResult =
                new MultiCallExpression(
                    new ObjectContextPropertyCall(
                        new ObjectContextPropertyCall(
                            new CurrentContextPropertyCall("var1")
                            , "var2"
                            ), "var3"
                        )
                    );

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TestArrayExpression()
        {
            var result = AstBuilder.Build("array[12].Item");


            var expectedResult =
                new MultiCallExpression(
                    new ObjectContextPropertyCall(
                        new ArrayElementCall(
                            new CurrentContextPropertyCall("array"),
                            new LiteralExpression(new Int32Literal(12))
                            ),
                        "Item"
                        )
                    );

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TestArrayExpression_Nested()
        {
            var result = AstBuilder.Build("array[12][3]");


            var expectedResult =
                new MultiCallExpression(
                    new ArrayElementCall(
                        new ArrayElementCall(
                            new CurrentContextPropertyCall("array"),
                            new LiteralExpression(new Int32Literal(12))
                            ),
                        new LiteralExpression(new Int32Literal(3))
                        )
                    );

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TestUnaryOperatorNegate()
        {
            var result = AstBuilder.Build("-2 + 1");

            var expectedResult =
                new BinaryExpression(
                    new UnaryExpression(UnaryOperator.Negate, new LiteralExpression(new Int32Literal(2))),
                    BinaryOperator.Add,
                    new LiteralExpression(new Int32Literal(1)));

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TestUnaryOperatorAndParenthesis()
        {
            var result = AstBuilder.Build("-(2+2)+2");


            var expectedResult =
                new BinaryExpression(
                    new UnaryExpression(
                        UnaryOperator.Negate,
                        new BinaryExpression(
                            new LiteralExpression(new Int32Literal(2)),
                            BinaryOperator.Add,
                            new LiteralExpression(new Int32Literal(2)))),
                    BinaryOperator.Add,
                    new LiteralExpression(new Int32Literal(2)));

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TestUnaryOperatorIdentity()
        {
            var result = AstBuilder.Build("+2 + 1");


            var expectedResult =
                new BinaryExpression(
                    new UnaryExpression(UnaryOperator.Identity, new LiteralExpression(new Int32Literal(2))),
                    BinaryOperator.Add,
                    new LiteralExpression(new Int32Literal(1)));

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TestUnaryOperatorLogicalNegate()
        {
            // TODO: this is actually is not correct, because LogicalNegate can be applied only to bool value
            var result = AstBuilder.Build("!2 + 1");


            var expectedResult =
                new BinaryExpression(
                    new UnaryExpression(UnaryOperator.LogicalNegate, new LiteralExpression(new Int32Literal(2))),
                    BinaryOperator.Add,
                    new LiteralExpression(new Int32Literal(1)));

            Assert.AreEqual(expectedResult, result);
        }
    }
}