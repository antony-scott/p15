using p15.Core.Messages;

namespace p15.Core.Services
{
    public class TraceService
    {        
        private readonly IMessagingService _messagingService;

        public TraceService(IMessagingService messagingService)
        {
            _messagingService = messagingService;
        }

        public void Error(string msg) => Trace("Error", msg);
        public void Info(string msg) => Trace("Info", msg);
        public void Warn(string msg) => Trace("Warn", msg);
        public void Debug(string msg) => Trace("Debug", msg);

        private void Trace(string level, string msg)
        {
            // TODO: introduce "colour" here (and in the view via a binding)
            _messagingService
                .SendMessage(new TraceOutputMessage
                {
                    Level = level,
                    Trace = msg
                });
        }
    }
}
