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
