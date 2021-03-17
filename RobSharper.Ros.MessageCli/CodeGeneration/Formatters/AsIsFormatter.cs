namespace RobSharper.Ros.MessageCli.CodeGeneration.Formatters
{
    public class AsIsFormatter : INameFormatter
    {
        public string Format(string value)
        {
            return value;
        }
    }
}