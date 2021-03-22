using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace RobSharper.Ros.MessageCli.Configuration
{
    [Verb("config-output", HelpText = "View or edit the default output format for generated message packages.")]
    public class OutputConfigurationOptions
    {
        public enum ConfigurationElements
        {
            Nupkg,
            Dll
        }
        
        private string _valueString;

        [Value(1, MetaName = "Value",
            HelpText = "Output format for generated message packages: nupkg | dll",
            Required = false)]
        public string ValueString
        {
            get => _valueString;
            set
            {
                _valueString = value;

                if (!string.IsNullOrEmpty(value))
                {
                    if (!Enum.TryParse(typeof(ConfigurationElements), value, true, out var configElement))
                    {
                        throw new NotSupportedException($"Value '{value}' is not supported");
                    }

                    Value = (ConfigurationElements) configElement;
                }
            }
        }

        public ConfigurationElements? Value { get; set; }
        
        
        [Usage(ApplicationAlias = Program.Name)]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>() {
                    new Example("Show default output (dll or nupkg)", new OutputConfigurationOptions()),
                    new Example("Set default output (dll or nupkg)", new OutputConfigurationOptions { ValueString = "nupkg"}),
                };
            }
        }
    }
}