using System;
using System.IO;

namespace RobSharper.Ros.MessageCli.CodeGeneration
{
    public readonly struct RosPackageFolder
    {
        public enum BuildType
        {

            Mandatory,
            Optional
        }
        
        public string Path { get; }
        
        public BuildType BuildStrategy { get; }

        public RosPackageFolder(string path, BuildType type)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            
            path = System.IO.Path.GetFullPath(path);
            
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"Directory {path} does not exit.");
            }
            
            Path = path;
            BuildStrategy = type;
        }
    }
}