// Copyright (c) Diyou. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Runtime.Versioning;

namespace AppImage.Update;

[SupportedOSPlatform("linux")]
public class Updater
{
    private static string appImageLocation => Environment.GetEnvironmentVariable("APPIMAGE");
    public static bool AppIsAppImage => appImageLocation != null;

    private Native.Updater handle;
    public Updater(string pathToAppImage, bool overwrite = false)
    {
        try
        {
            handle = new Native.Updater(pathToAppImage, overwrite);
        }
        catch (Exception e)
        {
            throw new ArgumentException(e.Message, e);
        }
    }
}
