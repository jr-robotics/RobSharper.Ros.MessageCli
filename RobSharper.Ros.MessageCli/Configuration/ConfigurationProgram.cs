using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace RobSharper.Ros.MessageCli.Configuration
{
    public static class ConfigurationProgram
    {

        public static void Execute(NamespaceConfigurationOptions options)
        {
            var configuration = LoadConfiguration();

            var value = options.Value?.Trim();

            if (string.IsNullOrEmpty(value))
            {
                Console.WriteLine(configuration.RootNamespace ?? "<not set>");
            }
            else
            {
                configuration.RootNamespace = options.Value;
                UpdateConfiguration(configuration);
                
                Console.WriteLine($"Set namespace to {configuration.RootNamespace}");
            }
        }

        public static void Execute(OutputConfigurationOptions options)
        {
            var configuration = LoadConfiguration();

            if (!options.Value.HasValue)
            {
                Console.WriteLine(configuration.DefaultBuildAction ?? "<not set>");
            }
            else
            {
                var value = options.Value.ToString().ToLowerInvariant();
                
                configuration.DefaultBuildAction = value;
                UpdateConfiguration(configuration);
                
                Console.WriteLine($"Set output to {configuration.DefaultBuildAction}");
            }
        }

        public static void Execute(CodeGeneratorConfigurationOptions options)
        {
            var configuration = LoadConfiguration();

            if (!options.Value.HasValue)
            {
                Console.WriteLine(configuration.CodeGenerator ?? "<not set>");
            }
            else
            {
                var value = options.Value.ToString().ToLowerInvariant();
                
                configuration.CodeGenerator = value;
                configuration.RootNamespace = CodeGeneratorConfigurationOptions.DefaultNamespaces[options.Value.Value];
                UpdateConfiguration(configuration);
                
                Console.WriteLine($"Set codegenerator to {configuration.CodeGenerator}");
                Console.WriteLine($"Set namespace to {configuration.RootNamespace}");
            }
        }
        
        public static void Execute(FeedConfigurationOptions options)
        {
            var configuration = LoadConfiguration();
            
            switch (options.Command)
            {
                case FeedConfigurationOptions.Commands.Show:
                    ShowFeeds(options, configuration);
                    break;
                case FeedConfigurationOptions.Commands.Add:
                    AddFeed(options, configuration);
                    break;
                case FeedConfigurationOptions.Commands.Remove:
                    RemoveFeed(options, configuration);
                    break;
            }
        }

        private static void ShowFeeds(FeedConfigurationOptions options, CodeGenerationConfiguration configuration)
        {
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
                        
                Console.WriteLine($"{"Name",-24} Source");
                foreach (var feed in configuration.NugetFeeds)
                {
                    var protocolVersionSuffix = feed.ProtocolVersion > 0 ? $" (protocol {feed.ProtocolVersion})" : "";
                    Console.WriteLine($"{feed.Name,-24} {feed.Source}{protocolVersionSuffix}");
                }
            }
        }

        
        private static void RemoveFeed(FeedConfigurationOptions options, CodeGenerationConfiguration configuration)
        {
            var feedName = options.Name?.Trim();

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
        }

        private static void AddFeed(FeedConfigurationOptions options, CodeGenerationConfiguration configuration)
        {
            var feedName = options.Name?.Trim();

            if (string.IsNullOrEmpty(feedName))
            {
                Console.WriteLine("No feed name provided");
                Environment.ExitCode |= (int) ExitCodes.InvalidFeedName;
                return;
            }

            var source = options.Source?.Trim();
            if (string.IsNullOrEmpty(source))
            {
                Console.WriteLine("No feed source provided");
                Environment.ExitCode |= (int) ExitCodes.InvalidFeedSource;
                return;
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
        }

        private static CodeGenerationConfiguration LoadConfiguration()
        {
            var configFilePath = GetConfigFilePath();

            var configJson = File.ReadAllText(configFilePath);
            var config = JsonConvert.DeserializeObject<ConfigurationRootElement>(configJson);

            return config.Build;
        }

        private static string GetConfigFilePath()
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
            var configFilePath = GetConfigFilePath();
            
            File.WriteAllText(configFilePath, serialized);
        }
        
        
    }
    
}