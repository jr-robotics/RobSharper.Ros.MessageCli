using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace RobSharper.Ros.MessageCli.ColorfulConsoleLogging
{
    public static class ColorfulConsoleLoggerExtensions
    {
        public static ILoggingBuilder AddColorfulConsole(
            this ILoggingBuilder builder)
        {
            builder.AddConfiguration();

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILoggerProvider, ColorfulConsoleLoggerProvider>());

            LoggerProviderOptions.RegisterProviderOptions
                <ColorfulConsoleLoggerConfiguration, ColorfulConsoleLoggerProvider>(builder.Services);

            return builder;
        }

        public static ILoggingBuilder AddColorfulConsole(
            this ILoggingBuilder builder,
            Action<ColorfulConsoleLoggerConfiguration> configure)
        {
            builder.AddColorfulConsole();
            builder.Services.Configure(configure);

            return builder;
        }
    }
}