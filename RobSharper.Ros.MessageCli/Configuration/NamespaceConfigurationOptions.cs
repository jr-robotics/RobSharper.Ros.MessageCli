using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace RobSharper.Ros.MessageCli.Configuration
{
    [Verb("config-namespace", HelpText = "View or edit the default root namespace for generated message packages.")]
    public class NamespaceConfigurationOptions
    {
        [Value(1, MetaName = "Value", HelpText = "Namespace to set.", Required = false)]
        public string Value { get; set; }
        
        
        [Usage(ApplicationAlias = Program.Name)]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>() {
                    new Example("Show message root namespace", new NamespaceConfigurationOptions()),
                    new Example("Set message root namespace", new NamespaceConfigurationOptions { Value = "My.Messages.Namespace"}),
                };
            }
        }
    }
}