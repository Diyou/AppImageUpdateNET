using System.Diagnostics;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Mono.Options;

namespace AppImage.Update;

[SupportedOSPlatform("linux")]
internal static class UpdaterTool
{
    private enum Exit
    {
        Success = 0,
        Failure = 1
    }

    private static List<string>? extra;

    public static int Main(string[] args)
    {
        AppImageLogger logger = new AppImageLogger();

        string appName = Path.GetFileName(Process.GetCurrentProcess().MainModule?.FileName)??"AppImageUpdateTool";
        string? selfAppImagePath = Environment.GetEnvironmentVariable("APPIMAGE");

        void showHelp(OptionSet p, string app)
        {
            Console.WriteLine("AppImage companion tool taking care of updates for the commandline.\n");
            Console.WriteLine($"Usage: {app} [options...] [<path to AppImage>]");
            p.WriteOptionDescriptions(Console.Out);
        }

        bool shouldShowHelp = false;
        bool showVersion = false;
        bool selfUpdate = false;
        bool describe = false;
        bool checkUpdate = false;
        bool overwrite = false;
        bool removeOld = false;

        var p = new OptionSet
        {
            { "h|help", "show this message and exit", h => shouldShowHelp = h != null },
            { "V|version", "Display version information.", v => showVersion = v != null },
            { "d|describe", "Parse and describe AppImage and its update information and exit.", d => describe = d != null },
            { "j|check-for-update", "Check for update. Exits with code 1 if changes are available, 0 if there are not,other non-zero code in case of errors.", c => checkUpdate = c != null },
            { "O|overwrite", "Overwrite existing file. If not specified, a new file will be created, and the old one will remain untouched.", o => overwrite = o != null },
            { "r|remove-old", "Remove old AppImage after successful update.", r => removeOld = r != null },
        };

        // option is unavailable if APPIMAGE is not set
        if (selfAppImagePath != null)
        {
            p.Add("self-update", "Update this AppImage.", su => selfUpdate = su != null);
        }

        try
        {
            extra = p.Parse(args);
        }
        catch (OptionException e)
        {
            logger.Write($"{appName}: {e.Message}\nTry `{appName} --help' for more information.", LogLevel.Warn);
            return (int)Exit.Failure;
        }

        if (shouldShowHelp)
        {
            showHelp(p, appName);
            return (int)Exit.Success;
        }

        if (showVersion)
        {
            logger.Write($"{appName} version {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion}", LogLevel.Info);
            return (int)Exit.Success;
        }

        if (selfUpdate)
        {
            if (extra.Count > 0)
            {
                logger.Write("Error: --self-update does not take a path", LogLevel.Error);
                showHelp(p, appName);
                return (int)Exit.Failure;
            }

            if (!File.Exists(selfAppImagePath))
            {
                logger.Write("Error: $APPIMAGE pointing to non-existing file", LogLevel.Error);
                return (int)Exit.Failure;
            }
        }

        string pathToAppImage = extra[0];

        if (string.IsNullOrEmpty(pathToAppImage))
        {
            showHelp(p, appName);
            return (int)Exit.Failure;
        }

        if (pathToAppImage.StartsWith('~'))
        {
            var r = new Regex(Regex.Escape("~"));
            string relativePath = pathToAppImage[2..];
            pathToAppImage = r.Replace("~", $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/", 1);
            pathToAppImage += relativePath;
        }

        if (!File.Exists(pathToAppImage))
        {
            logger.Write($"Could not read file: {pathToAppImage}", LogLevel.Fatal);
            return (int)Exit.Failure;
        }

        try
        {
            var updater = new Updater(new UpdaterOptions
            {
                AppPath = pathToAppImage,
                Overwrite = overwrite,
                RemoveOldFile = removeOld
            })
            {
                Logger = logger
            };

            if (describe)
            {
                string description = updater.Describe();

                if (description == null) return (int)Exit.Failure;

                logger.Write($"\n{description}", LogLevel.Info);
                return (int)Exit.Success;
            }

            bool? hasUpdates = updater.HasUpdates();

            if (checkUpdate)
            {
                if (hasUpdates.HasValue)
                {
                    return hasUpdates.Value ? 1 : 0;
                }

                return 2;
            }

            logger.Write(updater.Describe(), LogLevel.Debug);

            if (hasUpdates.HasValue && hasUpdates.Value)
            {
                updater.Download(progress =>
                {
                    logger.Write($"{progress * 100}", LogLevel.Info);
                }).GetAwaiter().GetResult();
                bool? validated = updater.Validate();
                return (int)(validated.HasValue && validated.Value ? Exit.Success : Exit.Failure);
            }
        }
        catch (Exception e)
        {
            logger.Write(e.Message, LogLevel.Fatal);
            return (int)Exit.Failure;
        }

        return (int)Exit.Success;
    }

    private class AppImageLogger : ILogger
    {
        public void Dispose()
        {
        }
        public LogLevel Level { get; set; } = LogLevel.Debug;
        public void Write(string message, LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                case LogLevel.Warn:
                    Console.Out.WriteLine($"{message}");
                    break;

                case LogLevel.Error:
                case LogLevel.Fatal:
                    Console.Error.WriteLine($"{Enum.GetName(typeof(LogLevel), logLevel)}: {message}");
                    break;
            }
        }
    }
}
