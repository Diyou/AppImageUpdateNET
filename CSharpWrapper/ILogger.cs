// Copyright (c) Diyou. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace AppImage.Update
{
    public enum LogLevel
    {
        Debug = 1,
        Info = 2,
        Warn = 3,
        Error = 4,
        Fatal = 5,
    }

    public interface ILogger : IDisposable
    {
        public LogLevel Level { get; set; }

        public void Write(string message, LogLevel logLevel);
    }
}


