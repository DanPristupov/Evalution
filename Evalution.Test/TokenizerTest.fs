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
            Bracket '(';
            Integer 42;
            Operator '+';
            Integer 31;
            Bracket ')';
        ]
        Assert.AreEqual(expectedResult, result)

    [<TestMethod>]
    member x.TestTokenReader3 ()=
        let result = tokenizer.Read "(42+31.0)"
        
        let expectedResult = [
            Bracket '(';
            Integer 42;
            Operator '+';
            Double 31.0;
            Bracket ')';
        ]
        Assert.AreEqual(expectedResult, result)

    [<TestMethod>]
    member x.TestTokenReader4 ()=
        let result = tokenizer.Read "(42+ 8) *2"
        
        let expectedResult = [
            Bracket '(';
            Integer 42;
            Operator '+';
            Integer 8;
            Bracket ')';
            Operator '*';
            Integer 2;
        ]
        Assert.AreEqual(expectedResult, result)

    [<TestMethod>]
    member x.TestTokenReader5 ()=
        let result = tokenizer.Read "(42.87+31.0"
        
        let expectedResult = [
            Bracket '(';
            Double 42.87;
            Operator '+';
            Double 31.0;
        ]
        Assert.AreEqual(expectedResult, result)

    [<TestMethod>]
    member x.TestTokenReader6 ()=
        let result = tokenizer.Read "(prop2+31.0"
        
        let expectedResult = [
            Bracket '(';
            Identifier "prop2";
            Operator '+';
            Double 31.0;
        ]
        Assert.AreEqual(expectedResult, result)

    [<TestMethod>]
    member x.TestTokenReader_ComplexObject ()=
        let result = tokenizer.Read "(prop2.Value1.SubValue2+31.0"
        
        let expectedResult = [
            Bracket '(';
            Identifier "prop2";
            CallMember;
            Identifier "Value1";
            CallMember;
            Identifier "SubValue2";
            Operator '+';
            Double 31.0;
        ]
        Assert.AreEqual(expectedResult, result)

