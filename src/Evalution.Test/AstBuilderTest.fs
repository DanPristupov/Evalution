namespace Evalution.Test
open Evalution
open NUnit.Framework

[<TestFixture>]
type AstBuilderTest() =
    
    [<Test>]
    member x.TestAddition ()=
        let result = AstBuilder.build "1+2"
        
        let expectedResult =
            Ast.BinaryExpression(
                Ast.LiteralExpression(Ast.Int32Literal(1)),
                Ast.Add,
                Ast.LiteralExpression(Ast.Int32Literal(2)))

        Assert.AreEqual(expectedResult, result)

    [<Test>]
    member x.TestSubtract ()=
        let result = AstBuilder.build "1-2"
        
        let expectedResult =
            Ast.BinaryExpression(
                Ast.LiteralExpression(Ast.Int32Literal(1)),
                Ast.Subtract,
                Ast.LiteralExpression(Ast.Int32Literal(2)))

        Assert.AreEqual(expectedResult, result)

    [<Test>]
    member x.TestMultiply ()=
        let result = AstBuilder.build "1*2"
        
        let expectedResult =
            Ast.BinaryExpression(
                Ast.LiteralExpression(Ast.Int32Literal(1)),
                Ast.Multiply,
                Ast.LiteralExpression(Ast.Int32Literal(2)))

        Assert.AreEqual(expectedResult, result)

    [<Test>]
    member x.TestDivide ()=
        let result = AstBuilder.build "1/2"
        
        let expectedResult =
            Ast.BinaryExpression(
                Ast.LiteralExpression(Ast.Int32Literal(1)),
                Ast.Divide,
                Ast.LiteralExpression(Ast.Int32Literal(2)))

        Assert.AreEqual(expectedResult, result)

    [<Test>]
    member x.TestInt32Literal ()=
        let result = AstBuilder.build "1"
        
        let expectedResult =
            Ast.LiteralExpression(Ast.Int32Literal(1))

        Assert.AreEqual(expectedResult, result)

    [<Test>]
    member x.TestDoubleLiteral ()=
        let result = AstBuilder.build "1.5"
        
        let expectedResult =
            Ast.LiteralExpression(Ast.DoubleLiteral(1.5))

        Assert.AreEqual(expectedResult, result)

    [<Test>]
    member x.TestBinaryExpressionInt32DivideDouble ()=
        let result = AstBuilder.build "3/2"
        
        let expectedResult =
            Ast.BinaryExpression(
                Ast.LiteralExpression(Ast.Int32Literal(3)),
                Ast.Divide,
                Ast.LiteralExpression(Ast.Int32Literal(2)))

        Assert.AreEqual(expectedResult, result)

    [<Test>]
    member x.TestSpaces ()=
        let result = AstBuilder.build "  1.5 +	2 "
        
        let expectedResult =
            Ast.BinaryExpression(
                Ast.LiteralExpression(Ast.DoubleLiteral(1.5)),
                Ast.Add,
                Ast.LiteralExpression(Ast.Int32Literal(2)))

        Assert.AreEqual(expectedResult, result)

    [<Test>]
    member x.TestParentheses()=
        let result = AstBuilder.build "( 1.5 + 1 ) * 2"
        
        let expectedResult =
            Ast.BinaryExpression(
                Ast.BinaryExpression(
                    Ast.LiteralExpression(Ast.DoubleLiteral(1.5)),
                    Ast.Add,
                    Ast.LiteralExpression(Ast.Int32Literal(1))),
                Ast.Multiply,
                Ast.LiteralExpression(Ast.Int32Literal(2)))

        Assert.AreEqual(expectedResult, result)

    [<Test>]
    member x.TestMethodCallExpression ()=
        let result = AstBuilder.build "method(1.0,2+3)"

        let expectedResult =
            Ast.MultiCallExpression (
                Ast.CurrentContextMethodCall(
                    "method",
                    [
                        Ast.LiteralExpression(
                            Ast.DoubleLiteral(1.0)
                        );
                        Ast.BinaryExpression(
                            Ast.LiteralExpression(Ast.Int32Literal(2)),
                            Ast.Add,
                            Ast.LiteralExpression(Ast.Int32Literal(3))
                        )
                    ]
                )
            )

        Assert.AreEqual(expectedResult, result)

    [<Test>]
    member x.TestMethodChainCallExpression1 ()=
        let result = AstBuilder.build "Method1(1).Method2(2)"

        let expectedResult =
            Ast.MultiCallExpression(
                Ast.ObjectContextMethodCall(
                    Ast.CurrentContextMethodCall(
                        "Method1", [Ast.LiteralExpression(Ast.Int32Literal(1))]
                    ), "Method2", [Ast.LiteralExpression(Ast.Int32Literal(2))]
                )
            )

        Assert.AreEqual(expectedResult, result)

    [<Test>]
    member x.TestMethodChainCallExpression2 ()=
        let result = AstBuilder.build "Method1(1).Prop1.Method2(2)"

        let expectedResult =
            Ast.MultiCallExpression(
                Ast.ObjectContextMethodCall(
                    Ast.ObjectContextPropertyCall(
                        Ast.CurrentContextMethodCall(
                            "Method1", [Ast.LiteralExpression(Ast.Int32Literal(1))]
                        ),
                        "Prop1" ),
                    "Method2", [Ast.LiteralExpression(Ast.Int32Literal(2))]
                )
            )

        Assert.AreEqual(expectedResult, result)

    [<Test>]
    member x.TestMethodCallExpression_NoArguments ()=
        let result = AstBuilder.build "method()"

        let expectedResult =
            Ast.MultiCallExpression (
                Ast.CurrentContextMethodCall(
                    "method",
                    []
                )
            )

        Assert.AreEqual(expectedResult, result)

    [<Test>]
    member x.TestMultiCallExpression_Single ()=
        let result = AstBuilder.build "var1"
        
        let expectedResult =
            Ast.MultiCallExpression(
                Ast.CurrentContextPropertyCall("var1"))

        Assert.AreEqual(expectedResult, result)

    [<Test>]
    member x.TestMultiCallExpression_Multiple ()=
        let result = AstBuilder.build "var1.var2.var3"
        
        let expectedResult =
            Ast.MultiCallExpression(
                Ast.ObjectContextPropertyCall(
                    Ast.ObjectContextPropertyCall(
                        Ast.CurrentContextPropertyCall("var1")
                        , "var2"
                    ), "var3"
                )
            )

        Assert.AreEqual(expectedResult, result)

    [<Test>]
    member x.TestArrayExpression ()=
        let result = AstBuilder.build "array[12].Item"
        
        let expectedResult =
            Ast.MultiCallExpression(
                Ast.ObjectContextPropertyCall(
                    Ast.ArrayElementCall(
                        Ast.CurrentContextPropertyCall("array"),
                        Ast.LiteralExpression(Ast.Int32Literal(12))
                    ),
                    "Item"
                )
            )

        Assert.AreEqual(expectedResult, result)

    [<Test>]
    member x.TestArrayExpression_Nested ()=
        let result = AstBuilder.build "array[12][3]"
        
        let expectedResult =
            Ast.MultiCallExpression(
                Ast.ArrayElementCall(
                    Ast.ArrayElementCall(
                        Ast.CurrentContextPropertyCall("array"),
                        Ast.LiteralExpression(Ast.Int32Literal(12))
                    ),
                    Ast.LiteralExpression(Ast.Int32Literal(3))
                )
            )

        Assert.AreEqual(expectedResult, result)

    [<Test>]
    member x.TestUnaryOperatorNegate()=
        let result = AstBuilder.build "-2 + 1"
        
        let expectedResult =
            Ast.BinaryExpression(
                Ast.UnaryExpression(Ast.Negate, Ast.LiteralExpression(Ast.Int32Literal(2))),
                Ast.Add,
                Ast.LiteralExpression(Ast.Int32Literal(1)))

        Assert.AreEqual(expectedResult, result)

    [<Test>]
    member x.TestUnaryOperatorAndParenthesis()=
        let result = AstBuilder.build "-(2+2)+2"
        
        let expectedResult =
            Ast.BinaryExpression(
                Ast.UnaryExpression(
                    Ast.Negate,
                    Ast.BinaryExpression(
                        Ast.LiteralExpression(Ast.Int32Literal(2)),
                        Ast.Add,
                        Ast.LiteralExpression(Ast.Int32Literal(2)))),
                Ast.Add,
                Ast.LiteralExpression(Ast.Int32Literal(2)))

        Assert.AreEqual(expectedResult, result)

    [<Test>]
    member x.TestUnaryOperatorIdentity()=
        let result = AstBuilder.build "+2 + 1"
        
        let expectedResult =
            Ast.BinaryExpression(
                Ast.UnaryExpression(Ast.Identity, Ast.LiteralExpression(Ast.Int32Literal(2))),
                Ast.Add,
                Ast.LiteralExpression(Ast.Int32Literal(1)))

        Assert.AreEqual(expectedResult, result)

    [<Test>]
    member x.TestUnaryOperatorLogicalNegate()=
        // TODO: this is actually is not correct, because LogicalNegate can be applied only to bool value
        let result = AstBuilder.build "!2 + 1"
        
        let expectedResult =
            Ast.BinaryExpression(
                Ast.UnaryExpression(Ast.LogicalNegate, Ast.LiteralExpression(Ast.Int32Literal(2))),
                Ast.Add,
                Ast.LiteralExpression(Ast.Int32Literal(1)))

        Assert.AreEqual(expectedResult, result)
