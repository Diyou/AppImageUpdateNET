using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Options;

// ReSharper disable once CheckNamespace
namespace AppImage.Update;

internal static class UpdaterTool
{
    private static int verbosity;

    public static void Main(string[] args)
    {
        string appName = Path.GetFileName(Process.GetCurrentProcess()?.MainModule?.FileName)??"AppImageUpdateTool";

        bool shouldShowHelp = false;
        var names = new List<string>();
        int repeat = 1;

        var p = new OptionSet
        {
            { "n|name=", "the name of someone to greet.", n => names.Add(n) },
            { "r|repeat=", "the number of times to repeat the greeting.", (int r) => repeat = r },
            {
                "v", "increase debug message verbosity", v =>
                {
                    if (v != null) ++verbosity;
                }
            },
            { "h|help", "show this message and exit", h => shouldShowHelp = h != null },
            { "V|version"}
        };

        List<string> extra;
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
