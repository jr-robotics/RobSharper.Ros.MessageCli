using System;
using System.Drawing;
using Microsoft.Extensions.Logging;

namespace RobSharper.Ros.MessageCli.ColorfulConsoleLogging
{
    public class ColorfulConsoleLogger : ILogger
    {
        private readonly string _name;
        private readonly ColorfulConsoleLoggerConfiguration _config;

        public ColorfulConsoleLogger(string name, ColorfulConsoleLoggerConfiguration config)
        {
            _name = name;
            _config = config;
        }
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;
            
            Color? color = null;

            switch (logLevel)
            {
                case LogLevel.Critical:
                case LogLevel.Error:
                    color = Color.Red;
                    break;
                case LogLevel.Warning:
                    color = Color.Orange;
                    break;
            }

            var message = formatter(state, exception);
            WriteLine(message, color);
            
            if (_config.LogStackTrace && exception != null)
                WriteLine(exception.ToString());
        }

        private static void WriteLine(string message, Color? color = null)
        {
            if (color.HasValue)
            {
                Colorful.Console.WriteLine(message, color.Value);
            }
            else
            {
                Colorful.Console.WriteLine(message);
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Information:
                case LogLevel.Warning:
                case LogLevel.Error:
                case LogLevel.Critical:
                    return true;
                default:
                    return false;
            }
        }

        public IDisposable BeginScope<TState>(TState state) => default;
    }
}