using System;
using RobSharper.Ros.MessageParser;

namespace RobSharper.Ros.MessageCli.CodeGeneration.MessagePackage.TemplateData
{
    public class FieldTypeTemplateData
    {
        public RosTypeInfo TypeInfo { get; }
        public string InterfaceName { get; set; }
        public string ConcreteName { get; set; }

        public bool IsBuiltInType => TypeInfo.IsBuiltInType;
        
        
        public bool IsArray => TypeInfo.IsArray;
        public int ArraySize => TypeInfo.ArraySize;
        public bool IsVariableSizeArray => IsArray && ArraySize == 0;
        public bool IsFixedSizeArray => IsArray && ArraySize > 0;
        

        public bool IsValueType => TypeInfo.IsValueType();
        public bool SupportsEqualityComparer => TypeInfo.SupportsEqualityComparer();
        public bool IsString => TypeInfo.IsType<string>();
        
        public bool IsDateTime => TypeInfo.IsType<DateTime>();
        public bool IsTimeSpan => TypeInfo.IsType<TimeSpan>();
        public bool IsDateTimeOrTimeSpan => IsDateTime || IsTimeSpan;

        public FieldTypeTemplateData(string interfaceName, string concreteName, RosTypeInfo typeInfo)
        {
            InterfaceName = interfaceName;
            ConcreteName = concreteName;
            
            TypeInfo = typeInfo;
        }
    }
}