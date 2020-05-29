using System;

namespace RobSharper.Ros.MessageCli
{
    [Flags]
    public enum ExitCodes
    {
        Success = 0,
        InvalidConfiguration = 1 << 0,
        UnhandledException = 1 << 1,
        RosPackagePathNotFound = 1 << 2,
        ConfigurationElementNotSupported = 1 << 3,
        InvalidFeedName = 1 << 4,
        InvalidFeedSource = 1 << 5,
        CouldNotAddDependency = 1 << 6,
        CouldNotProcessPackage = 1 << 7,
        CouldNotDetermineBuildSequence = 1 << 8
    }
}