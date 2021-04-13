using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RobSharper.Ros.MessageCli.ColorfulConsoleLogging
{
    [ProviderAlias("ColorfulConsole")]
    public sealed class ColorfulConsoleLoggerProvider : ILoggerProvider
    {
        private readonly IDisposable _onChangeToken;
        private ColorfulConsoleLoggerConfiguration _currentConfig;
        private readonly ConcurrentDictionary<string, ColorfulConsoleLogger> _loggers = new ConcurrentDictionary<string, ColorfulConsoleLogger>();
        
        public ColorfulConsoleLoggerProvider(
            IOptionsMonitor<ColorfulConsoleLoggerConfiguration> config)
        {
            _currentConfig = config.CurrentValue;
            _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        }
        
        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new ColorfulConsoleLogger(name, _currentConfig));

        public void Dispose()
        {
            _loggers.Clear();
            _onChangeToken.Dispose();
        }
    }
}