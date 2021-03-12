namespace RobSharper.Ros.MessageCli.CodeGeneration.MessagePackage
{
    public class ServiceTemplateData
    {
        public PackageTemplateData Package { get; set; }
        public ConcreteTypeTemplateData ServiceType { get; set; }
        public ConcreteTypeTemplateData RequestType { get; set; }
        public ConcreteTypeTemplateData ResponseType { get; set; }
    }
}