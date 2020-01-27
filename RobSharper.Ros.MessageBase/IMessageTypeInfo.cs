using System.Collections.Generic;

namespace RobSharper.Ros.MessageBase
{
    public interface IMessageTypeInfo
    {
        RosMessageDescriptor MessageDescriptor { get; }
        
        IEnumerable<IMessageTypeInfo> Dependencies { get; }
        
        string MD5Sum { get; }
        
        string MessageDefinition { get; }
    }
}