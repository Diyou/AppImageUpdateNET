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
    public void EmptyString()
    {
        Assert.Catch<Exception>(() =>
        {
            var updater = new Updater("");
        });
    }

    [Test]
    [SupportedOSPlatform("linux")]
    public void ValidPath()
    {
        Assert.DoesNotThrow(() =>
        {
            var updater = new Updater("test.AppImage");
        });
    }
}
