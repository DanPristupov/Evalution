namespace UnitTestProject1

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open Evalution

[<TestClass>]
type SyntaxTreeTest() =

    [<TestMethod>]
    member x.TestBuildSyntaxTree1 ()=
        let syntaxTree = new SyntaxTree()

        let result = syntaxTree.Build [
            Integer 42;
            Operator '+';
            Integer 8;
        ]
        
        let expectedResult = Addition (Const (CInteger 8), Const (CInteger 42))
        Assert.AreEqual(expectedResult, result)

    [<TestMethod>]
    member x.TestBuildSyntaxTree2 ()=
        let syntaxTree = new SyntaxTree()

        let result = syntaxTree.Build [
            Operator '(';
            Integer 42;
            Operator '+';
            Integer 8;
            Operator ')';
            Operator '*';
            Integer 2;
        ]
        
        let expectedResult = [
            Integer 42;
            Operator '+';
            Integer 31;
        ]
        Assert.AreEqual(expectedResult, result)
 