namespace RobSharper.Ros.MessageCli.CodeGeneration.MessagePackage.TemplateData
{
    public class FieldTypeTemplateData
    {
        public string InterfaceName { get; set; }
        public string ConcreteName { get; set; }
        public bool IsBuiltInType { get; set; }
        public bool IsArray { get; set; }
        public bool IsValueType { get; set; }
        public bool SupportsEqualityComparer { get; set; }
    }
}