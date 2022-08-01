using System;
using System.Runtime.Versioning;
using System.Threading;
using AppImage.Update.Native;
using NUnit.Framework;

// ReSharper disable UnusedVariable
namespace UpdaterTests;

/// <summary>
/// Tests appimage::update::Updater in a dotnet runtime
/// </summary>
[SupportedOSPlatform("linux")]
public class UpdaterClass
{
    private Updater updater;
    [SetUp]
    public void Setup()
    {
        updater = new Updater("../../../../appimagetool-x86_64.AppImage", false);
        Assert.IsTrue(updater.state() == Updater.State.INITIALIZED);
        string newFilePath = null;
        Assert.IsFalse(updater.pathToNewFile(ref newFilePath));
    }

    [Test]
    public void ConstructWithInvalidPath()
    {
        var e = Assert.Catch<Exception>(() =>
        {
            var u = new Updater("");
        });
        TestContext.Out.WriteLine(e);
    }
    [Test]
    public void UpdateInformation()
    {
        string updateInformation = updater.updateInformation();
        Assert.IsFalse(string.IsNullOrEmpty(updateInformation));
        TestContext.Out.WriteLine(updateInformation);
    }

    [Test]
    public void DescribeAppImage()
    {
        string description = null;
        updater.describeAppImage(ref description);
        Assert.IsFalse(string.IsNullOrEmpty(description));
        TestContext.Out.WriteLine(description);
    }

    [Test]
    public void CheckForChanges()
    {
        bool hasUpdates = false;
        Assert.IsTrue(updater.checkForChanges(ref hasUpdates));
        TestContext.WriteLine(hasUpdates);
    }

    [Test]
    public void Update()
    {
        Assert.IsTrue(updater.start());

        bool threadFinished = false;

        int remoteFileSize = 0;
        double progress = 0;
        string newFile = null;

        Thread tr = new Thread(() =>
        {
            while (!threadFinished)
            {
                updater.progress(ref progress);
                updater.remoteFileSize(ref remoteFileSize);

                Thread.Sleep(100);

                TestContext.Out.WriteLine($"Bytes: {remoteFileSize} ➡ {Math.Round(remoteFileSize / Math.Pow(1024, 2), 2)}MB");

                if (updater.isDone()) threadFinished = true;
                TestContext.Out.WriteLine($"State: {updater.state()}");
            }
        });
        tr.Start();
        tr.Join();

        var validation = updater.validateSignature();
        TestContext.Out.WriteLine($"Validation State: {validation}");
        TestContext.Out.WriteLine(Updater.signatureValidationMessage(validation));

        Assert.IsTrue(updater.pathToNewFile(ref newFile));
        TestContext.Out.WriteLine(newFile);
    }
}
