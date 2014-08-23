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
