using System.Collections.Generic;

namespace RobSharper.Ros.MessageCli.CodeGeneration.MessagePackage.TemplateData
{
    public class MessageTemplateData
    {
        public PackageTemplateData Package { get; set; }
            
        public string RosTypeName { get; set; }
        public string RosAbstractTypeName { get; set; }
            
        public string TypeName { get; set; }
        public string AbstractTypeName { get; set; }
            
        public IList<FieldTemplateData> Fields { get; set; }
            
        public IList<ConstantTemplateData> Constants { get; set; }
            
        public MessageTypeTemplateData MessageType { get; set; }
    }
}