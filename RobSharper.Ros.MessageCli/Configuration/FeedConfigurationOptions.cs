using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace RobSharper.Ros.MessageCli.Configuration
{
    [Verb("config-feeds", HelpText = "View or edit nuget feed configuration options.")]
    public class FeedConfigurationOptions
    {
        public enum Commands
        {
            Show,
            Add,
            Remove
        }
        
        private string _commandString;

        [Value(1, MetaName = "Command", HelpText = "show | add | remove", Required = false, Default = "show")]
        public string CommandString
        {
            get => _commandString;
            set
            {
                _commandString = value;

                if (string.IsNullOrEmpty(_commandString))
                {
                    Command = Commands.Show;
                }
                else
                {
                    if (!Enum.TryParse(typeof(Commands), value, true, out var configElement))
                    {
                        throw new NotSupportedException($"Command '{value}' is not supported");
                    };

                    Command = (Commands) configElement;
                }
            }
        }

        public Commands Command { get; set; }
        
        
        [Value(2, MetaName = "Name", HelpText = "The name of the nuget feed", Required = false)]
        public string Name { get; set; }
        
        [Value(3, MetaName = "Source", HelpText = "Nuget feed source", Required = false)]
        public string Source { get; set; }
        
        [Value(4, MetaName = "Protocol", Required = false, HelpText = "Optional nuget protocol version (e.g. 3)")]
        public int ProtocolVersion { get; set; }
        
        
        [Usage(ApplicationAlias = Program.Name)]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>() {
                    new Example("List nuget feeds", new FeedConfigurationOptions { CommandString = "show"}),
                    new Example("Add nuget feed", new FeedConfigurationOptions { CommandString = "add", Name = "My NuGet feed", Source = "https://robotics-baget.joanneum.at/v3/index.json", ProtocolVersion = 3}),
                    new Example("Add nuget feed", new FeedConfigurationOptions { CommandString = "add", Name = "My NuGet feed", Source = "https://robotics-baget.joanneum.at/v3/index.json"}),
                    new Example("Add nuget feed", new FeedConfigurationOptions { CommandString = "add", Name = "My NuGet feed", Source = "/path/to/nuget/files/"}),
                    new Example("Remove nuget feed", new FeedConfigurationOptions { CommandString = "remove", Name = "My NuGet feed"}),
                };
            }
        }
    }
}