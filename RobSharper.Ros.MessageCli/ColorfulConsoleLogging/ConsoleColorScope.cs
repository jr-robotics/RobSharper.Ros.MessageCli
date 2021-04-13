using System;
using System.Drawing;

namespace RobSharper.Ros.MessageCli.ColorfulConsoleLogging
{
    public class ConsoleColorScope : IDisposable
    {
        public static ConsoleColorScope Current { get; private set; }
        
        public Color Color { get; }
        
        private ConsoleColorScope Previous { get; }

        public ConsoleColorScope(Color color)
        {
            Color = color;

            lock (typeof(ConsoleColorScope))
            {
                Previous = Current;
                Current = this;
            }
        }

        public void Dispose()
        {
            lock (typeof(ConsoleColorScope))
            {
                Current = Previous;
            }
        }
    }
}