using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace RobSharper.Ros.MessageCli
{
    public static class LoggingHelper
    {
        private static ILoggerFactory _factory = NullLoggerFactory.Instance;

        public static ILoggerFactory Factory
        {
            get => _factory;
            set => _factory = value ?? NullLoggerFactory.Instance;
        }
    }
}