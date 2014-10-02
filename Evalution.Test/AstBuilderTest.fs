namespace Evalution.Test
open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open Evalution

[<TestClass>]
type AstBuilderTest() =
    
    [<TestMethod>]
    member x.TestAddition ()=
        let result = AstBuilder.build "1+2"
        
        let expectedResult =
            Ast.BinaryExpression(
                Ast.LiteralExpression(Ast.Int32Literal(1)),
                Ast.Add,
                Ast.LiteralExpression(Ast.Int32Literal(2)))

        Assert.AreEqual(expectedResult, result)

    [<TestMethod>]
    member x.TestSubtract ()=
        let result = AstBuilder.build "1-2"
        
        let expectedResult =
            Ast.BinaryExpression(
                Ast.LiteralExpression(Ast.Int32Literal(1)),
                Ast.Subtract,
                Ast.LiteralExpression(Ast.Int32Literal(2)))

        Assert.AreEqual(expectedResult, result)

    [<TestMethod>]
    member x.TestMultiply ()=
        let result = AstBuilder.build "1*2"
        
        let expectedResult =
            Ast.BinaryExpression(
                Ast.LiteralExpression(Ast.Int32Literal(1)),
                Ast.Multiply,
                Ast.LiteralExpression(Ast.Int32Literal(2)))

        Assert.AreEqual(expectedResult, result)

    [<TestMethod>]
    member x.TestDivide ()=
        let result = AstBuilder.build "1/2"
        
        let expectedResult =
            Ast.BinaryExpression(
                Ast.LiteralExpression(Ast.Int32Literal(1)),
                Ast.Divide,
                Ast.LiteralExpression(Ast.Int32Literal(2)))

        Assert.AreEqual(expectedResult, result)

    [<TestMethod>]
    member x.TestInt32Literal ()=
        let result = AstBuilder.build "1"
        
        let expectedResult =
            Ast.LiteralExpression(Ast.Int32Literal(1))

        Assert.AreEqual(expectedResult, result)

    [<TestMethod>]
    member x.TestDoubleLiteral ()=
        let result = AstBuilder.build "1.5"
        
        let expectedResult =
            Ast.LiteralExpression(Ast.DoubleLiteral(1.5))

        Assert.AreEqual(expectedResult, result)

    [<TestMethod>]
    member x.TestBinaryExpressionInt32DivideDouble ()=
        let result = AstBuilder.build "3/2"
        
        let expectedResult =
            Ast.BinaryExpression(
                Ast.LiteralExpression(Ast.Int32Literal(3)),
                Ast.Divide,
                Ast.LiteralExpression(Ast.Int32Literal(2)))

        Assert.AreEqual(expectedResult, result)
