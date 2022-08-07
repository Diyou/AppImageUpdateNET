// Copyright (c) Diyou. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

#pragma warning disable CS1591

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
        /// <summary>
        /// The lowest level to be passed
        /// </summary>
        public LogLevel Level { get; set; }

        internal void Filter(string message, LogLevel logLevel)
        {
            if (logLevel < Level) return;

            Write(message, logLevel);
        }

        public void Write(string message, LogLevel logLevel);
    }

    internal class GenericLogger : ILogger
    {
        public void Dispose()
        {
        }
        public LogLevel Level { get; set; } = LogLevel.Fatal;
        public void Write(string message, LogLevel logLevel)
        {
        }
    }
}


