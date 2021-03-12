using System;
using System.Collections.Generic;
using System.Linq;

namespace RobSharper.Ros.MessageCli.CodeGeneration.MessagePackage.TemplateData
{
    public class PackageTemplateData
    {
        public string RosName { get; set; }
        public string Version { get; set; }
            
        /// <summary>
        /// .Net Name of the ROS package
        /// </summary>
        public string Name { get; set; }
            
        /// <summary>
        /// .Net Namespace of the ROS package
        /// </summary>
        public string Namespace { get; set; }
            
        public string Description { get; set; }

        public bool HasDescription => !string.IsNullOrEmpty(Description);
            
        public string ProjectUrl { get; set; }

        public bool HasProjectUrl => !string.IsNullOrEmpty(ProjectUrl);
            
        public string RepositoryUrl { get; set; }

        public bool HasRepositoryUrl => !string.IsNullOrEmpty(RepositoryUrl);
            
        public IEnumerable<string> Authors { get; set; }

        public bool HasAuthors => Authors != null && Authors.Any();
            
        public Func<IEnumerable<string>, string> AuthorsToStringFunc { get; set; } = authors => string.Join(";", authors ?? Enumerable.Empty<string>());

        public string AuthorsString => AuthorsToStringFunc(Authors);
    }
}