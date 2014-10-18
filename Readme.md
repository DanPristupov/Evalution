##Evalution

Evalution is an evaluation library which allows to inject math expressions in a property during runtime.

Evalution is not an expression interpretator. It's a JIT-compiler. It parses an expression, builds abstract syntax tree and then generates the IL code. The generated code is as fast as a native .Net code.

Currently Evalution supports Int32 and Double types. TimeSpan/DateTime and string operations will be implemented soon.

Evalution is written in F# (only because I want to learn the language :)).

Example:

```C#
    [Test]
    public void GeneralTest_Double()
    {
        var classBuilder = new ClassBuilder<ClassDouble>()
            .Setup(x => x.ValueWithExpression,  "2.0 + 2.0 * 2.5")
            .Setup(x => x.DependentValue1,      "Value1 * 2.0")
            .Setup(x => x.DependentValue2,      "DependentValue1 * 2.0");
        var target = classBuilder.BuildObject();

        target.Value1 = 1.5;
        Assert.AreEqual(7.0, target.ValueWithExpression);   // "2.0 + 2.0 * 2.5"
        Assert.AreEqual(3.0, target.DependentValue1);       // "Value1 * 2.0"
        Assert.AreEqual(6.0, target.DependentValue2);       // "DependentValue1 * 2.0"
    }
```

There's also a non-generic API:

```C#
    [Test]
    public void GeneralTest_NonGeneric()
    {
        var classBuilder = new ClassBuilder(typeof(ClassInt32))
            .Setup("ValueWithExpression",   "2 + 2 * 2")
            .Setup("DependentValue1",       "Value1 * 2")
            .Setup("DependentValue2",       "DependentValue1 * 2");
        var target = (ClassInt32)classBuilder.BuildObject();
        
        target.Value1 = 4;
        Assert.AreEqual(6,  target.ValueWithExpression);    // "2+2*2"
        Assert.AreEqual(8,  target.DependentValue1);        // "Value1*2"
        Assert.AreEqual(16, target.DependentValue2);        // "DependentValue1*2"
    }
```
