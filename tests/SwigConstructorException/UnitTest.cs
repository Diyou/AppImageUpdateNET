using System;
using NUnit.Framework;
using AppImage.Update;

namespace SwigConstructorException;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestConstruction()
    {
        Assert.Throws<ArgumentException>(() =>
            {
                var updater = new Updater("");
            });
    }
}