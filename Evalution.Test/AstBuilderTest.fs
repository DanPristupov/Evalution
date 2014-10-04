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
        let result = AstBuilder.build "(1.5 + 1) * 2"
        
        let expectedResult =
            Ast.LiteralExpression(Ast.DoubleLiteral(1.5))

        Assert.AreEqual(expectedResult, result)

    [<Test>]
    member x.TestIdentifierExpression ()=
        let result = AstBuilder.build "variable/2"
        
        let expectedResult =
            Ast.BinaryExpression(
                Ast.IdentifierExpression(Ast.Identifier("variable")),
                Ast.Divide,
                Ast.LiteralExpression(Ast.Int32Literal(2)))

        Assert.AreEqual(expectedResult, result)
