// Copyright (c) Diyou. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace AppImage.Update;

using State = Native.Updater.State;
using ValidationState = Native.Updater.ValidationState;

[SupportedOSPlatform("linux")]
public sealed class UpdaterOptions
{
    public string AppPath;
    public bool Overwrite;
    public bool RemoveOldFile;

    public UpdaterOptions()
    {
        AppPath = Updater.AppImageLocation;
        Overwrite = false;
        RemoveOldFile = false;
    }
}

[SupportedOSPlatform("linux")]
public class Updater
{
    public static string AppImageLocation => Environment.GetEnvironmentVariable("APPIMAGE");

    /// <summary>
    /// Checks whether the application was launched as an AppImage via the $APPIMAGE Environment Variable.'
    /// </summary>
    public static bool IsAppImage => AppImageLocation != null;

    private readonly Native.Updater handle;
    private UpdaterOptions options { get; }

    private bool? hasUpdates;
    public Updater(UpdaterOptions options)
    {
        this.options = options;

        try
        {
            handle = new Native.Updater(Path.GetFullPath(options.AppPath), options.Overwrite);
        }
        catch (Exception e)
        {
            throw new ArgumentException(e.Message, e);
        }
    }
    public Updater(string pathToAppImage, bool overwrite = false)
        : this(new UpdaterOptions { AppPath = pathToAppImage, Overwrite = overwrite })
    {
    }

    /// <summary>
    /// Checks for updates with the embedded Update Information
    /// </summary>
    /// <returns>
    /// null if parsing fails
    /// </returns>
    public string Describe()
    {
        string description = null;
        bool success = handle.describeAppImage(ref description);

        checkForStatusMessages();

        if (success) return description;

        Logger.Write("Failed to parse AppImage. See above for more information", LogLevel.Fatal);
        return null;
    }

    /// <summary>
    /// Checks for updates with the embedded Update Information
    /// </summary>
    /// <returns>
    /// null if parsing fails
    /// </returns>
    public bool? HasUpdates()
    {
        if (handle.state() != State.INITIALIZED)
        {
            Logger.Write("Update already started.", LogLevel.Warn);
            return hasUpdates;
        }

        bool checkForUpdates = false;

        if (handle.checkForChanges(ref checkForUpdates))
        {
            hasUpdates = checkForUpdates;
        }
        else
        {
            hasUpdates = null;
            Logger.Write("Update check Failed", LogLevel.Error);
        }

        checkForStatusMessages();

        if (hasUpdates.HasValue && hasUpdates.Value)
        {
            Logger.Write("Update available.", LogLevel.Info);
        }
        else
        {
            Logger.Write("No Updates available.", LogLevel.Debug);
        }

        return hasUpdates;
    }

    public ConfiguredTaskAwaitable<State> Update => Task.Run(() =>
    {
        handle.start();
        Logger.Write("Started Downloading", LogLevel.Debug);

        var timer = new Timer(100);
        var reset = new ManualResetEvent(false);
        timer.Elapsed += (sender, args) =>
        {
            double progress = 0;

            checkForStatusMessages();

            if (!handle.progress(ref progress))
            {
                Logger.Write("Error occured while retrieving progress information", LogLevel.Error);
                reset.Set();
            }

            if (handle.isDone()) reset.Set();
        };
        timer.Start();
        reset.WaitOne();
        timer.Stop();

        checkForStatusMessages();

        if (handle.hasError())
        {
            Logger.Write("Update failed", LogLevel.Error);
        }

        return handle.state();
    }).ConfigureAwait(false);

    /// <summary>
    /// Post download validation.
    /// </summary>
    /// <param name="strict">Allows only </param>
    /// <returns></returns>
    public bool? Validate(bool strict = false)
    {
        if (handle.state() != State.SUCCESS)
        {
            Logger.Write("Can not validate before a successful download.", LogLevel.Error);
            return null;
        }

        string newFilePath = null;

        if (!handle.pathToNewFile(ref newFilePath))
        {
            Logger.Write("Could not determine path to new file", LogLevel.Error);
            return null;
        }

        ValidationState validationState = Task.Run(() =>
            handle.validateSignature()
        ).ConfigureAwait(false).GetAwaiter().GetResult();

        checkForStatusMessages();

        bool failed()
        {
            handle.restoreOriginalFile();
            Logger.Write($"Validation failed: {Native.Updater.signatureValidationMessage(validationState)}", LogLevel.Error);
            Logger.Write("Restoring original file", LogLevel.Error);
            return false;
        }

        bool success()
        {
            handle.copyPermissionsToNewFile();
            Logger.Write("Signature validation passed", LogLevel.Debug);

            if (options.RemoveOldFile)
            {
                string oldFilePath = Path.GetFullPath(options.AppPath);
                if (oldFilePath.Equals(Path.GetFullPath(newFilePath))) oldFilePath += ".zs-old";

                if (File.Exists(oldFilePath))
                {
                    try
                    {
                        File.Delete(oldFilePath);
                        Logger.Write($"Deleting: {oldFilePath}", LogLevel.Debug);
                    }
                    catch (Exception e)
                    {
                        Logger.Write(e.Message, LogLevel.Error);
                    }
                }
                else
                {
                    Logger.Write($"Could not find old AppImage: {oldFilePath}", LogLevel.Warn);
                }
            }

            Logger.Write("Update successful.", LogLevel.Info);
            return true;
        }

        if (validationState >= ValidationState.VALIDATION_FAILED) return failed();

        if (strict)
        {
            return validationState == ValidationState.VALIDATION_PASSED ? success() : failed();
        }

        if (validationState >= ValidationState.VALIDATION_WARNING)
        {
            if (validationState == ValidationState.VALIDATION_NOT_SIGNED)
            {
                handle.copyPermissionsToNewFile();
            }

            Logger.Write($"Validation warning: {Native.Updater.signatureValidationMessage(validationState)}", LogLevel.Warn);
        }

        if (validationState is <= ValidationState.VALIDATION_WARNING or ValidationState.VALIDATION_NOT_SIGNED) return success();

        return failed();
    }

    private void checkForStatusMessages()
    {
        string statusMessage = null;

        while (handle.nextStatusMessage(ref statusMessage))
        {
            Logger.Write(statusMessage, LogLevel.Debug);
        }
    }

    public ILogger Logger { get; init; }
}