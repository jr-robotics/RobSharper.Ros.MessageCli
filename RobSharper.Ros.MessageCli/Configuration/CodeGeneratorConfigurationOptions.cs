using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace RobSharper.Ros.MessageCli.Configuration
{
    [Verb("config-codegenerator", HelpText = "View or edit the default code generator")]
    public class CodeGeneratorConfigurationOptions
    {
        public enum ConfigurationElements
        {
            RobSharper,
            UmlRobotics
        }
        
        private string _valueString;

        [Value(1, MetaName = "Value",
            HelpText = "Code generator for package generation: robsharper | umlrobotics",
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
                    new Example("Show configured code generator", new CodeGeneratorConfigurationOptions()),
                    new Example("Set code generator to RobSharper ", new CodeGeneratorConfigurationOptions { ValueString = "robsharper"}),
                    new Example("Set code generator to UmlRobotics ", new CodeGeneratorConfigurationOptions { ValueString = "umlrobotics"}),
                };
            }
        }
    }
}