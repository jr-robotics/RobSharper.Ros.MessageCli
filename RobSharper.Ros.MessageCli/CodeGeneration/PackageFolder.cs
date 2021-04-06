using System;
using System.IO;

namespace RobSharper.Ros.MessageCli.CodeGeneration
{
    public readonly struct PackageFolder
    {
        public enum BuildType
        {

            Mandatory,
            Optional
        }
        
        public string Path { get; }
        
        public BuildType Strategy { get; }

        public PackageFolder(string path, BuildType type)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            
            path = System.IO.Path.GetFullPath(path);
            
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"Directory {path} does not exit.");
            }
            
            Path = path;
            Strategy = type;
        }
    }
}