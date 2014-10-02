namespace Evalution.Test

open System
open NUnit.Framework
open Evalution

[<TestFixture>]
type EvaluatorTest() =
    let tokenizer = new Tokenizer()
    let syntaxTree = new SyntaxTree()

    [<Test>]
    member x.EvaluateTest1 ()=
        let tokens = tokenizer.Read "2+3"
        let expression = syntaxTree.Build tokens

        let evaluator = new Evaluator();
        let result = evaluator.Evaluate(expression);
        Assert.AreEqual(5.0, result)

    [<Test>]
    member x.EvaluateTest2 ()=
        let tokens = tokenizer.Read "2+3+1"
        let expression = syntaxTree.Build tokens

        let evaluator = new Evaluator();
        let result = evaluator.Evaluate(expression);
        Assert.AreEqual(6.0, result)

    [<Test>]
    member x.EvaluateTest3 ()=
        let tokens = tokenizer.Read "2+3*2"
        let expression = syntaxTree.Build tokens

        let evaluator = new Evaluator();
        let result = evaluator.Evaluate(expression);
        Assert.AreEqual(8.0, result)

    [<Test>]
    member x.EvaluateTest4 ()=
        let expression1 = syntaxTree.Build (tokenizer.Read "1+3*2")

        let expression2 = syntaxTree.Build (tokenizer.Read "3*2+1")

        let evaluator = new Evaluator();
        Assert.AreEqual(evaluator.Evaluate(expression1), evaluator.Evaluate(expression2))

    [<Test>]
    member x.EvaluateTest5 ()=
        let tokens = tokenizer.Read "(2+3)*2"
        let expression = syntaxTree.Build tokens

        let evaluator = new Evaluator();
        let result = evaluator.Evaluate(expression);
        Assert.AreEqual(10.0, result)

    [<Test>]
    member x.EvaluateTest6 ()=
        let tokens = tokenizer.Read "1+(2+3)*2"
        let expression = syntaxTree.Build tokens

        let evaluator = new Evaluator();
        let result = evaluator.Evaluate(expression);
        Assert.AreEqual(11.0, result)

    [<Test>]
    member x.EvaluateTest7 ()=
        let tokens = tokenizer.Read "(-1)"
        let expression = syntaxTree.Build tokens

        let evaluator = new Evaluator();
        let result = evaluator.Evaluate(expression);
        Assert.AreEqual(-1.0, result)

    [<Test>]
    member x.EvaluateTest8 ()=
        let tokens = tokenizer.Read "-(-1)-1"
        let expression = syntaxTree.Build tokens

        let evaluator = new Evaluator();
        let result = evaluator.Evaluate(expression);
        Assert.AreEqual(0, result)
