namespace Evalution.Test
open System
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
    member x.TestTimeSpanLiteral ()=
        let result = AstBuilder.build "TimeSpan.FromHours(4.0 * 2)"
        
        let expectedResult =
            Ast.LiteralExpression(Ast.TimeSpanLiteral(TimeSpan.FromHours(8.0)))

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

//    [<Test>]
//    member x.TestIdentifierExpression ()=
//        let result = AstBuilder.build "variable/2"
//        
//        let expectedResult =
//            Ast.BinaryExpression(
//                Ast.IdentifierExpression(Ast.Identifier("variable")),
//                Ast.Divide,
//                Ast.LiteralExpression(Ast.Int32Literal(2)))
//
//        Assert.AreEqual(expectedResult, result)
//
    [<Test>]
    member x.TestMultiCallExpression_Single ()=
        let result = AstBuilder.build "var1"
        
        let expectedResult =
            Ast.MultiCallExpression(
                Ast.ThisPropertyCall(Ast.Identifier("var1")))

        Assert.AreEqual(expectedResult, result)

    [<Test>]
    member x.TestMultiCallExpression_Multiple ()=
        let result = AstBuilder.build "var1.var2.var3"
        
        let expectedResult =
            Ast.MultiCallExpression(
                Ast.ObjectPropertyCall(
                    Ast.ObjectPropertyCall(
                        Ast.ThisPropertyCall(Ast.Identifier("var1"))
                        , Ast.Identifier("var2")
                    ), Ast.Identifier("var3")
                )
            )

        Assert.AreEqual(expectedResult, result)

    [<Test>]
    member x.TestArrayExpression ()=
        let result = AstBuilder.build "array[12].Item"
        
        let expectedResult =
            Ast.MultiCallExpression(
                Ast.ObjectPropertyCall(
                    Ast.ArrayElementCall(
                        Ast.ThisPropertyCall(Ast.Identifier("array")),
                        Ast.LiteralExpression(Ast.Int32Literal(12))
                    ),
                    Ast.Identifier("Item")
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
                        Ast.ThisPropertyCall(Ast.Identifier("array")),
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
