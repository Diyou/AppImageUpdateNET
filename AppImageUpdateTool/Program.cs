using System.Diagnostics;
using Mono.Options;

// ReSharper disable once CheckNamespace
namespace AppImage.Update;

internal static class UpdaterTool
{
    private static List<string>? extra;
    public static void Main(string[] args)
    {
        string appName = Path.GetFileName(Process.GetCurrentProcess().MainModule?.FileName)??"AppImageUpdateTool";
        string? selfAppImagePath = Environment.GetEnvironmentVariable("APPIMAGE");

        bool shouldShowHelp = false;

        var p = new OptionSet
        {
            { "h|help", "show this message and exit", h => shouldShowHelp = h != null },
            { "V|version", "Display version information.", v => { } },
            { "d|describe", "Parse and describe AppImage and its update information and exit.", d => { } },
            { "j|check-for-update", "Check for update. Exits with code 1 if changes are available, 0 if there are not,other non-zero code in case of errors.", c => { } },
            { "O|overwrite", "Overwrite existing file. If not specified, a new file will be created, and the old one will remain untouched.", o => { } },
            { "r|remove-old", "Remove old AppImage after successful update.", r => { } },
        };

        if (selfAppImagePath != null)
        {
            p.Add("self-update", "Update this AppImage.", su => { });
        }

        try
        {
            extra = p.Parse(args);
        }
        catch (OptionException e)
        {
            Console.Write($"{appName}: ");
            Console.WriteLine(e.Message);
            Console.WriteLine($"Try `{appName} --help' for more information.");
            return;
        }

        if (shouldShowHelp)
        {
            showHelp(p, appName);
            return;
        }
    }

    private static void showHelp(OptionSet p, string appName)
    {
        Console.WriteLine("AppImage companion tool taking care of updates for the commandline.\n");
        Console.WriteLine($"Usage: {appName} [options...] [<path to AppImage>]");
        p.WriteOptionDescriptions(Console.Out);
    }
}
