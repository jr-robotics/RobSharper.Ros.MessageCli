namespace RobSharper.Ros.MessageCli.CodeGeneration.MessagePackage.TemplateData
{
    public class MessageTypeTemplateData
    {
        private readonly DetailedRosMessageType _messageType;

        public int MessageType => (int) _messageType;
        public bool IsMessage => _messageType.HasFlag(DetailedRosMessageType.Message);
        public bool IsAction => _messageType.HasFlag(DetailedRosMessageType.Action);
        public bool IsActionGoal => _messageType.HasFlag(DetailedRosMessageType.ActionGoal);
        public bool IsActionResult => _messageType.HasFlag(DetailedRosMessageType.ActionResult);
        public bool IsActionFeedback => _messageType.HasFlag(DetailedRosMessageType.ActionFeedback);
        public bool IsService => _messageType.HasFlag(DetailedRosMessageType.Service);
        public bool IsServiceRequest => _messageType.HasFlag(DetailedRosMessageType.ServiceRequest);
        public bool IsServiceResponse => _messageType.HasFlag(DetailedRosMessageType.ServiceResponse);

        public MessageTypeTemplateData(DetailedRosMessageType messageType)
        {
            _messageType = messageType;
        }
    }
}