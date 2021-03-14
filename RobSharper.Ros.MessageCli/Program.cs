﻿using System;
using System.Globalization;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CommandLine;
using CommandLine.Text;
using HandlebarsDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RobSharper.Ros.MessageCli.CodeGeneration;
using RobSharper.Ros.MessageCli.CodeGeneration.RosTargets.UmlRobotics;
using RobSharper.Ros.MessageCli.CodeGeneration.TemplateEngines;
using RobSharper.Ros.MessageCli.Configuration;

namespace RobSharper.Ros.MessageCli
{
    class Program
    {
        public const string Name = "dotnet-rosmsg";
        
        static void Main(string[] args)
        {
            var configuration = LoadConfiguration();

            using (var serviceProvider = CreateContainer(configuration))
            {
                LoggingHelper.Factory = serviceProvider.Resolve<ILoggerFactory>();

                var commandLineParser = new Parser(settings =>
                {
                    settings.HelpWriter = Console.Error;
                    settings.CaseInsensitiveEnumValues = true;
                });

                
                var parserResult = commandLineParser.ParseArguments<CodeGenerationOptions, FeedConfigurationOptions, NamespaceConfigurationOptions, OutputConfigurationOptions>(args);
                var hideUsage = false;

                parserResult
                    .WithParsed<CodeGenerationOptions>(options =>
                    {
                        var config = configuration.GetSection("Build");
                        var configObject = new CodeGenerationConfiguration();
                        config.Bind(configObject);

                        options.SetDefaultBuildAction(configObject.DefaultBuildAction);
                        options.SetDefaultRootNamespace(configObject.RootNamespace);
                        options.NugetFeedXmlSources = configObject.NugetFeeds?
                            .Select(f => f.GetXmlString())
                            .ToList() ?? Enumerable.Empty<string>();

                        var templateEngine = serviceProvider.Resolve<IKeyedTemplateFormatter>();
                        var packageGeneratorFactory = serviceProvider.ResolveKeyed<IRosPackageGeneratorFactory>(configObject.MessageGenerator);
                        
                        CodeGeneration.CodeGeneration.Execute(options, templateEngine, packageGeneratorFactory);
                    })
                    .WithParsed<FeedConfigurationOptions>(ConfigurationProgram.Execute)
                    .WithParsed<NamespaceConfigurationOptions>(ConfigurationProgram.Execute)
                    .WithParsed<OutputConfigurationOptions>(ConfigurationProgram.Execute)
                    .WithNotParsed(errs =>
                    {
                        hideUsage = true;
                        Environment.ExitCode |= (int) ExitCodes.InvalidConfiguration;
                    });

                
                if (!hideUsage && Environment.ExitCode != 0)
                {
                    PrintUsage(parserResult);
                }
            }
        }

        private static void PrintUsage(ParserResult<object> parserResult)
        {
            var h = HelpText.RenderUsageText(parserResult);

            if (string.IsNullOrEmpty(h))
                return;
            
            Console.WriteLine();
            Console.WriteLine("USAGE:");
            Console.WriteLine(h);
        }

        private static IContainer CreateContainer(IConfiguration configuration)
        {
            IServiceCollection services = new ServiceCollection();
            
            services
                .AddLogging(x => x
                    .AddConfiguration(configuration.GetSection("Logging"))
                    .AddDebug()
                    .AddConsole());
            
            var containerBuilder = new ContainerBuilder();
            
            containerBuilder.Populate(services);

            // Template Engine
            containerBuilder.Register(context =>
                {
                    var config = new HandlebarsConfiguration
                    {
                        ThrowOnUnresolvedBindingExpression = true,
                    };
                    
                    config.Helpers.Add("formatValue", (output, hbContext, arguments) =>
                    {
                        object value = arguments[0];

                        if (value is string)
                        {
                            output.WriteSafeString("\"");
                            output.WriteSafeString(value
                                .ToString()
                                .Replace("\t", "\\\t")
                                .Replace("\"", "\\\"")
                            );
                            output.WriteSafeString("\"");
                            return;
                        }

                        if (value is float)
                        {
                            output.WriteSafeString(string.Format(CultureInfo.InvariantCulture, "{0}", value));
                            output.WriteSafeString("f");
                            return;
                        }

                        if (value is bool)
                        {
                            output.WriteSafeString(string.Format(CultureInfo.InvariantCulture, "{0}", value).ToLowerInvariant());
                            return;
                        }
                        
                        output.WriteSafeString(string.Format(CultureInfo.InvariantCulture, "{0}", value));
                    });
                    return new FileBasedHandlebarsTemplateEngine(TemplatePaths.TemplatesDirectory, config);
                })
                .SingleInstance()
                .As<IKeyedTemplateEngine>()
                .As<IKeyedTemplateFormatter>();
            
            // Code generators
            containerBuilder.RegisterInstance(new UmlRoboticsMessagePackageGeneratorFactory())
                .SingleInstance()
                .Keyed<IRosPackageGeneratorFactory>("umlrobotics");
            
            var container = containerBuilder.Build();
            
            return container;
        }
        
        private static IConfigurationRoot LoadConfiguration()
        {
            IConfigurationRoot configuration;

            try
            {
                configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile("config.json")
                    .Build();
            }
            catch (Exception)
            {
                Console.WriteLine("Could not load configuration.");
                throw;
            }

            return configuration;
        }
    }
}