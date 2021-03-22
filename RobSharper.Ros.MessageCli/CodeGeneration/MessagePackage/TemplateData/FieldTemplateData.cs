using RobSharper.Ros.MessageParser;

namespace RobSharper.Ros.MessageCli.CodeGeneration.MessagePackage.TemplateData
{
    public class FieldTemplateData
    {
        public int Index { get; set; }
        public RosTypeInfo RosType { get; set; }
        public string RosIdentifier { get; set; }
            
        public FieldTypeTemplateData Type { get; set; }
        public string Identifier { get; set; }
    }
}