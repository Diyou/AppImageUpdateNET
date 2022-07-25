using System;
using System.Runtime.Versioning;
using AppImage.Update;
using NUnit.Framework;

// ReSharper disable UnusedVariable
namespace UpdaterTests;

[SupportedOSPlatform("linux")]
public class Tests
{
    private Updater _updater;
    [SetUp]
    public void Setup()
    {
        _updater = new Updater("../../../../appimagetool-x86_64.AppImage");
    }

    [Test]
    public void ConstructWithInvalidPath()
    {
        Assert.Catch<Exception>(() =>
        {
            var updater = new Updater("");
        });
    }
    [Test]
    public void UpdateInformation()
    {
        Assert.IsFalse(String.IsNullOrEmpty(_updater.updateInformation()));
    }

    [Test]
    public void DescribeAppImage()
    {
        string description = null;
        _updater.describeAppImage(ref description);
        Assert.IsFalse(String.IsNullOrEmpty(description));

    }
}