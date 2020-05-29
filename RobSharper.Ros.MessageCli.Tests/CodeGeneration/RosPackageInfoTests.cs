using System.IO;
using System.Linq;
using FluentAssertions;
using RobSharper.Ros.MessageCli.CodeGeneration;
using Xunit;

namespace RobSharper.Ros.MessageCli.Tests.CodeGeneration
{
    public class RosPackageInfoTests
    {
        [Fact]
        public void HasMessages_returns_true_if_package_contains_msg_file()
        {
            var packageFolder = TestUtils.CreatePackagePath("test_msg_msgs");
            var package = RosPackageInfo.Create(packageFolder);

            package.HasMessages.Should().BeTrue();
            package.Messages.Should().NotBeNull();
            package.Messages.Should().NotBeEmpty();
            package.Messages.Should().OnlyContain(f => f.GetRosMessageType() == RosMessageType.Message);
        }
        
        [Fact]
        public void HasMessages_returns_true_if_package_contains_srv_file()
        {
            var packageFolder = TestUtils.CreatePackagePath("test_srv_msgs");
            var package = RosPackageInfo.Create(packageFolder);

            package.HasMessages.Should().BeTrue();
            package.Messages.Should().NotBeNull();
            package.Messages.Should().NotBeEmpty();
            package.Messages.Should().OnlyContain(f => f.GetRosMessageType() == RosMessageType.Service);
        }
        
        [Fact]
        public void HasMessages_returns_true_if_package_contains_action_file()
        {
            var packageFolder = TestUtils.CreatePackagePath("test_action_msgs");
            var package = RosPackageInfo.Create(packageFolder);

            package.HasMessages.Should().BeTrue();
            package.Messages.Should().NotBeNull();
            package.Messages.Should().NotBeEmpty();
            package.Messages.Should().OnlyContain(f => f.GetRosMessageType() == RosMessageType.Action);
        }

        [Fact]
        void MessageFiles_returns_list_of_message_files()
        {
            var packageFolder = TestUtils.CreatePackagePath("std_msgs");
            var package = RosPackageInfo.Create(packageFolder);

            package.Should().NotBeNull();

            package.Messages.Should().NotBeNull();
            package.Messages.Count().Should().BeGreaterThan(0);
            package.Messages.Should().OnlyContain(f => f.GetRosMessageType() != RosMessageType.None);
        }
        
    }
}