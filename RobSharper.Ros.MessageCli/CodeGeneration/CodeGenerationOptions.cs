using System;
using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;
using RobSharper.Ros.MessageCli.Configuration;

namespace RobSharper.Ros.MessageCli.CodeGeneration
{
    [Verb("build", HelpText = "Generates .Net messages from ROS message packages")]
    public class CodeGenerationOptions
    {
        private string _rootNamespace;

        [Option("dll", Required = false, HelpText = "Create DLL (overrides default output configuration)")]
        public bool CreateDll { get; set; }
        
        [Option("nupkg", Required = false, HelpText = "Create nuget package (overrides default output configuration)")]
        public bool CreateNugetPackage { get; set; }
        
        [Option("preserve", Required = false, HelpText = "Preserve generated source code")]
        public bool PreserveGeneratedCode { get; set; }
        
        [Option('f', "filter", Required = false, HelpText = "Only generates packages matching the filter (e.g. '*_msgs' or 'geometry_msgs nav_msgs my_msgs')")]
        public IEnumerable<string> Filter { get; set; }

        [Option("namespace", Required = false, HelpText = "Root namespace (overrides default configuration)", Hidden = true)]
        public string RootNamespace
        {
            get => _rootNamespace;
            set => _rootNamespace = value?.Trim().TrimEnd('.');
        }
        
        public enum CodeGenerators
        {
            RobSharper,
            RosNet
        }
        
        private string _codeGeneratorString;

        [Option(longName: "CodeGenerator", Required = false, HelpText = "Used Code generator: robsharper | rosnet", Hidden = true)]
        public string CodeGeneratorString
        {
            get => _codeGeneratorString;
            set
            {
                _codeGeneratorString = value;

                if (!string.IsNullOrEmpty(value))
                {
                    if (!Enum.TryParse(typeof(CodeGenerators), value, true, out var configElement))
                    {
                        throw new NotSupportedException($"Value '{value}' is not supported");
                    }

                    CodeGenerator = (CodeGenerators) configElement;
                }
            }
        }

        public CodeGenerators? CodeGenerator { get; set; }


        [Value(0, MetaName = "PackagePath", HelpText = "ROS package(s) source folder", Required = true)]
        public string PackagePath { get; set; }
        
        [Value(1, MetaName = "OutputPath", HelpText = "Output path for generated packages", Required = true)]
        public string OutputPath { get; set; }

        public IEnumerable<string> NugetFeedXmlSources { get; set; }

        public void SetDefaultBuildAction(string defaultBuildOption)
        {
            if (CreateDll || CreateNugetPackage || defaultBuildOption == null)
                return;

            if ("nupkg".Equals(defaultBuildOption, StringComparison.InvariantCultureIgnoreCase))
            {
                CreateNugetPackage = true;
            }
            else if ("dll".Equals(defaultBuildOption, StringComparison.InvariantCultureIgnoreCase))
            {
                CreateDll = true;
            }
        }

        public void SetDefaultRootNamespace(string template)
        {
            if (string.IsNullOrEmpty(RootNamespace))
                RootNamespace = template;
        }

        public void SetDefaultCodeGenerator(string codeGenerator)
        {
            if (string.IsNullOrEmpty(CodeGeneratorString))
                CodeGeneratorString = codeGenerator;
        }
        
        [Usage(ApplicationAlias = Program.Name)]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>() {
                    new Example("Build nuget package", new CodeGenerationOptions { PackagePath = "./path/to/ros/message/package", OutputPath = "./nuget", CreateNugetPackage = true}),
                    new Example("Build dll", new CodeGenerationOptions { PackagePath = "./path/to/ros/message/package", OutputPath = "./lib", CreateDll = true}),
                    new Example("Build dll & nuget package", new CodeGenerationOptions { PackagePath = "./path/to/ros/message/package", OutputPath = "./bin", CreateDll = true, CreateNugetPackage = true}),
                };
            }
        }
    }
}