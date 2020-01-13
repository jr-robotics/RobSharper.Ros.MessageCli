using System.Collections.Generic;

namespace Joanneum.Robotics.Ros.MessageParser.Cli.CodeGeneration
{
    public interface IRosMessagePackageParser
    {
        RosPackageInfo Package { get; }
        IEnumerable<string> PackageDependencies { get; }
        IEnumerable<KeyValuePair<string, MessageDescriptor>> Messages { get; }
        IEnumerable<KeyValuePair<string, ActionDescriptor>> Actions { get; }
        IEnumerable<KeyValuePair<string, ServiceDescriptor>> Services { get; }
        void ParseMessages();
    }
}