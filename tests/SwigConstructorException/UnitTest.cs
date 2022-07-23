using System;
using System.Runtime.Versioning;
using AppImage.Update;
using NUnit.Framework;

namespace SwigConstructorException;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    [SupportedOSPlatform("linux")]
    public void TestExec()
    {
        Assert.Throws<ArgumentException>(Updater.testExec);
    }
        
    [Test]
    [SupportedOSPlatform("linux")]
    public void Construction()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var updater = new Updater("");
        });
    }
}