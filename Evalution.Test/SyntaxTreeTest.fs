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
                                    Integer 1;
                                    Operator '+';
                                    Integer 3;
                                    Operator '*';
                                    Integer 2;
        ]
        
        let expectedResult = Addition (
                                Multiplication (
                                    Const (CInteger 2),
                                    Const (CInteger 3)
                                ),
                                Const (CInteger 1)
        )
        Assert.AreEqual(expectedResult, result)

    [<TestMethod>]
    member x.TestBuildSyntaxTree3 ()=
        let syntaxTree = new SyntaxTree()

        let result = syntaxTree.Build [
                                    Integer 1;
                                    Operator '*';
                                    Integer 3;
                                    Operator '+';
                                    Integer 2;
        ]
        
        let expectedResult = Addition (
                                Const (CInteger 2),
                                Multiplication (
                                    Const (CInteger 3),
                                    Const (CInteger 1)
                                )
        )
        Assert.AreEqual(expectedResult, result)

    [<TestMethod>]
    member x.TestBuildSyntaxTree4 ()=
        let syntaxTree = new SyntaxTree()

        let result = syntaxTree.Build [
            Bracket '(';
            Integer 42;
            Operator '+';
            Integer 8;
            Bracket ')';
            Operator '*';
            Integer 2;
        ]
        let expectedResult = Multiplication (
                                Const (CInteger 2),
                                Addition (
                                    Const (CInteger 8),
                                    Const (CInteger 42)
                                ))
        Assert.AreEqual(expectedResult, result)
 