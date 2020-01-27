using System.IO;
using FluentAssertions;
using RobSharper.Ros.MessageBase.Serialization;
using Xunit;

namespace Joanneum.Robotics.Ros.MessageBase.Tests
{
    public class SerializationSpikes
    {
        [Fact]
        public void CanWriteAndReadInt()
        {
            using (var s = new MemoryStream())
            {
                var writer = new RosBinaryWriter(s);
                var reader = new RosBinaryReader(s);

                writer.Write((int) 4);
                writer.Write((int) 5);
                writer.Write((int) 6);

                s.Position = 0;

                reader.ReadInt32().Should().Be(4);
                reader.ReadInt32().Should().Be(5);
                reader.ReadInt32().Should().Be(6);
            }
        }
        
        [Fact]
        public void CanWriteAndReadString()
        {
            using (var s = new MemoryStream())
            {
                var writer = new RosBinaryWriter(s);
                var reader = new RosBinaryReader(s);

                writer.Write("ASDF");

                s.Position = 0;

                reader.ReadString().Should().Be("ASDF");
            }
        }
    }
}