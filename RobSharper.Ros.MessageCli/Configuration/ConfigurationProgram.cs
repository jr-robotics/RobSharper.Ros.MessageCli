using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace RobSharper.Ros.MessageCli.Configuration
{
    public static class ConfigurationProgram
    {
        public static void Execute(ConfigurationOptions options)
        {
            var configuration = LoadConfiguration();
            
            switch (options.Command)
            {
                case ConfigurationOptions.Commands.Show:
                    ShowConfigValues(options, configuration);
                    break;
                case ConfigurationOptions.Commands.Set:
                    SetConfigValue(options, configuration);
                    break;
                case ConfigurationOptions.Commands.Add:
                    AddConfigValue(options, configuration);
                    break;
                case ConfigurationOptions.Commands.Remove:
                    RemoveConfigValue(options, configuration);
                    break;
            }
        }

        private static void ShowConfigValues(ConfigurationOptions options, CodeGenerationConfiguration configuration)
        {
            switch (options.ConfigurationElement)
            {
                case ConfigurationOptions.ConfigurationElements.Namespace:
                    Console.WriteLine(configuration.RootNamespace ?? "<not set>");
                    break;
                case ConfigurationOptions.ConfigurationElements.DefaultOutput:
                    Console.WriteLine(configuration.DefaultBuildAction ?? "<not set>");
                    break;
                case ConfigurationOptions.ConfigurationElements.Feeds:
                    if (configuration.NugetFeeds == null || !configuration.NugetFeeds.Any())
                    {
                        Console.WriteLine("<not set>");
                    }
                    else
                    {
                        var maxNameLength = configuration
                            .NugetFeeds
                            .Select(f => f.Name.Length)
                            .Max();

                        const string NameHeader = "Name                    ";
                        
                        Console.WriteLine($"{"Name",-24} Source");
                        foreach (var feed in configuration.NugetFeeds)
                        {
                            var protocolVersionSuffix = feed.ProtocolVersion > 0 ? $" (protocol {feed.ProtocolVersion})" : "";
                            Console.WriteLine($"{feed.Name,-24} {feed.Source}{protocolVersionSuffix}");
                        }
                    }
                    break;
                default:
                    Console.WriteLine($"Show configuration element {options.ConfigurationElement} is not supported");
                    Environment.ExitCode |= (int) ExitCodes.ConfigurationElementNotSupported;
                    break;
            }
        }

        private static void SetConfigValue(ConfigurationOptions options, CodeGenerationConfiguration configuration)
        {
            var value = options.Value;
            switch (options.ConfigurationElement)
            {
                case ConfigurationOptions.ConfigurationElements.Namespace:
                    configuration.RootNamespace = value;
                    UpdateConfiguration(configuration);
                    break;
                case ConfigurationOptions.ConfigurationElements.DefaultOutput:
                    if (!"nupkg".Equals(value, StringComparison.InvariantCultureIgnoreCase) &&
                        !"dll".Equals(value, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine($"Set configuration element {options.ConfigurationElement} does not support value {value}");
                        Console.WriteLine("You can either choose 'nupkg' or 'dll'");
                        Environment.ExitCode |= (int) ExitCodes.ConfigurationElementNotSupported;
                    }
                    else
                    {
                        configuration.DefaultBuildAction = value;
                        UpdateConfiguration(configuration);
                    }
                    break;
                case ConfigurationOptions.ConfigurationElements.Feeds:
                    Console.WriteLine($"Set configuration element {options.ConfigurationElement} is not supported");
                    Console.WriteLine($"Use commands 'add' or 'remove'");
                    Environment.ExitCode |= (int) ExitCodes.ConfigurationElementNotSupported;
                    break;
                default:
                    Console.WriteLine($"Set configuration element {options.ConfigurationElement} is not supported");
                    Environment.ExitCode |= (int) ExitCodes.ConfigurationElementNotSupported;
                    break;
            }
        }

        private static void RemoveConfigValue(ConfigurationOptions options, CodeGenerationConfiguration configuration)
        {
            switch (options.ConfigurationElement)
            {
                case ConfigurationOptions.ConfigurationElements.Feeds:
                    var feedName = (options.Name ?? options.Value)?.Trim();

                    if (string.IsNullOrEmpty(feedName))
                    {
                        Console.WriteLine("No feed name provided");
                        Environment.ExitCode |= (int) ExitCodes.InvalidFeedName;
                    }
                    else
                    {
                        var feedItem = configuration.NugetFeeds.FirstOrDefault(x =>
                            x.Name.Equals(feedName, StringComparison.InvariantCultureIgnoreCase));

                        if (feedItem == null)
                        {
                            Console.WriteLine($"No feed with name {feedName} found");
                            Environment.ExitCode |= (int) ExitCodes.InvalidFeedName;
                        }
                        else
                        {
                            configuration.NugetFeeds.Remove(feedItem);
                            UpdateConfiguration(configuration);
                            
                            Console.WriteLine($"Nuget feed {feedName} removed");
                        }
                    }
                    
                    break;
                default:
                    Console.WriteLine($"Remove configuration element {options.ConfigurationElement} is not supported");
                    Environment.ExitCode |= (int) ExitCodes.ConfigurationElementNotSupported;
                    break;
            }
        }

        private static void AddConfigValue(ConfigurationOptions options, CodeGenerationConfiguration configuration)
        {
            switch (options.ConfigurationElement)
            {
                case ConfigurationOptions.ConfigurationElements.Feeds:
                    var feedName = (options.Name ?? options.Value)?.Trim();

                    if (string.IsNullOrEmpty(feedName))
                    {
                        Console.WriteLine("No feed name provided (use option -name)");
                        Environment.ExitCode |= (int) ExitCodes.InvalidFeedName;
                        break;
                    }

                    var source = options.Source?.Trim();
                    if (string.IsNullOrEmpty(source))
                    {
                        Console.WriteLine("No feed source provided (use option -source)");
                        Environment.ExitCode |= (int) ExitCodes.InvalidFeedSource;
                        break;
                    }
                    
                    var feedItemExists = configuration.NugetFeeds.Any(x =>
                        x.Name.Equals(feedName, StringComparison.InvariantCultureIgnoreCase));

                    if (feedItemExists)
                    {
                        Console.WriteLine($"Feed with name {feedName} already exists");
                        Environment.ExitCode |= (int) ExitCodes.InvalidFeedName;
                    }
                    else
                    {
                        var feedItem = new NugetSourceConfiguration
                        {
                            Name = feedName,
                            Source = source,
                            ProtocolVersion = options.ProtocolVersion
                        };
                        
                        configuration.NugetFeeds.Add(feedItem);
                        UpdateConfiguration(configuration);
                        
                        Console.WriteLine($"Nuget feed {feedName} added");
                    }
                    
                    break;
                default:
                    Console.WriteLine($"Remove configuration element {options.ConfigurationElement} is not supported");
                    Environment.ExitCode |= (int) ExitCodes.ConfigurationElementNotSupported;
                    break;
            }
        }

        private static CodeGenerationConfiguration LoadConfiguration()
        {
            var configFilePath = GetCOnfigFilePath();

            var configJson = File.ReadAllText(configFilePath);
            var config = JsonConvert.DeserializeObject<ConfigurationRootElement>(configJson);

            return config.Build;
        }

        private static string GetCOnfigFilePath()
        {
            var configFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "config.json");
            return configFilePath;
        }

        private static void UpdateConfiguration(CodeGenerationConfiguration configuration)
        {
            var container = new ConfigurationRootElement
            {
                Build = configuration
            };

            var serialized = JsonConvert.SerializeObject(container, Formatting.Indented);
            var configFilePath = GetCOnfigFilePath();
            
            File.WriteAllText(configFilePath, serialized);
        }
        
        
    }
    
}