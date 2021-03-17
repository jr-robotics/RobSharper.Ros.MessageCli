using RobSharper.Ros.MessageParser;

namespace RobSharper.Ros.MessageCli.CodeGeneration.MessagePackage.TemplateData
{
    public class ConstantTemplateData
    {
        public int Index { get; set; }
        public RosTypeInfo RosType { get; set; }
        public string RosIdentifier { get; set; }
        public string TypeName { get; set; }
        public string Identifier { get; set; }
        public object Value { get; set; }
    }
}