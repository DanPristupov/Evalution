namespace UnitTestProject1

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open Evalution

[<TestClass>]
type TokenizerTest() = 
    let tokenizer = new Tokenizer()

    [<TestMethod>]
    member x.TestTokenReader1 ()=
        let result = tokenizer.Read "42+31"
        
        let expectedResult = [
            Integer 42;
            Operator '+';
            Integer 31;
        ]
        Assert.AreEqual(expectedResult, result)

    [<TestMethod>]
    member x.TestTokenReader2 ()=
        let result = tokenizer.Read "(42+31)"
        
        let expectedResult = [
            Operator '(';
            Integer 42;
            Operator '+';
            Integer 31;
            Operator ')';
        ]
        Assert.AreEqual(expectedResult, result)

    [<TestMethod>]
    member x.TestTokenReader3 ()=
        let result = tokenizer.Read "(42+31.0)"
        
        let expectedResult = [
            Operator '(';
            Integer 42;
            Operator '+';
            Double 31.0;
            Operator ')';
        ]
        Assert.AreEqual(expectedResult, result)

    [<TestMethod>]
    member x.TestTokenReader4 ()=
        let result = tokenizer.Read "(42+ 8) *2"
        
        let expectedResult = [
            Operator '(';
            Integer 42;
            Operator '+';
            Integer 8;
            Operator ')';
            Operator '*';
            Integer 2;
        ]
        Assert.AreEqual(expectedResult, result)

    [<TestMethod>]
    member x.TestTokenReader5 ()=
        let result = tokenizer.Read "(42.87+31.0"
        
        let expectedResult = [
            Operator '(';
            Double 42.87;
            Operator '+';
            Double 31.0;
        ]
        Assert.AreEqual(expectedResult, result)

    [<TestMethod>]
    member x.TestTokenReader6 ()=
        let result = tokenizer.Read "(var+31.0"
        
        let expectedResult = [
            Operator '(';
            Integer 42;
            Operator '+';
            Integer 8;
            Operator ')';
            Operator '*';
            Integer 2;
        ]
        Assert.AreEqual(expectedResult, result)

    [<TestMethod>]
    member x.TestTokenReader7 ()=
        let result = tokenizer.Read "varb"
        
        let expectedResult = [
            Operator '(';
            Integer 42;
            Operator '+';
            Integer 8;
            Operator ')';
            Operator '*';
            Integer 2;
        ]
        Assert.AreEqual(expectedResult, result)

    [<TestMethod>]
    member x.TestTokenReader8 ()=
        let result = tokenizer.Read "varb("
        
        let expectedResult = [
            Operator '(';
            Integer 42;
            Operator '+';
            Integer 8;
            Operator ')';
            Operator '*';
            Integer 2;
        ]
        Assert.AreEqual(expectedResult, result)

    [<TestMethod>]
    member x.TestTokenReader9 ()=
        let result = tokenizer.Read "+varb("
        
        let expectedResult = [
            Operator '+';
            Integer 42;
            Operator '+';
            Integer 8;
            Operator ')';
            Operator '*';
            Integer 2;
        ]
        Assert.AreEqual(expectedResult, result)
