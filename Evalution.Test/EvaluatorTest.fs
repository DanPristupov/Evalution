namespace Evalution.Test

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open Evalution

[<TestClass>]
type EvaluatorTest() =

    [<TestMethod>]
    member x.EvaluateTest1 ()=
        let syntaxTree = new SyntaxTree()
        let expression = syntaxTree.Build [
            Integer 2;
            Operator '+';
            Integer 3;
        ]

        let evaluator = new Evaluator();
        let result = evaluator.Evaluate(expression);
        Assert.AreEqual(5.0, result)

    [<TestMethod>]
    member x.EvaluateTest2 ()=
        let syntaxTree = new SyntaxTree()
        let expression = syntaxTree.Build [
            Integer 2;
            Operator '+';
            Integer 3;
            Operator '+';
            Integer 1;
        ]

        let evaluator = new Evaluator();
        let result = evaluator.Evaluate(expression);
        Assert.AreEqual(6.0, result)

    [<TestMethod>]
    member x.EvaluateTest3 ()=
        let syntaxTree = new SyntaxTree()
        let expression = syntaxTree.Build [
            Integer 2;
            Operator '+';
            Integer 3;
            Operator '*';
            Integer 2;
        ]

        let evaluator = new Evaluator();
        let result = evaluator.Evaluate(expression);
        Assert.AreEqual(10.0, result)

    [<TestMethod>]
    member x.EvaluateTest4 ()=
        let syntaxTree = new SyntaxTree()
        let expression1 = syntaxTree.Build [
            Integer 1;
            Operator '+';
            Integer 3;
            Operator '*';
            Integer 2;
        ]

        let expression2 = syntaxTree.Build [
            Integer 3;
            Operator '*';
            Integer 2;
            Operator '+';
            Integer 1;
        ]

        let evaluator = new Evaluator();
        Assert.AreEqual(evaluator.Evaluate(expression1), evaluator.Evaluate(expression2))
