namespace p15.Core.Messages
{
    public class MonitorLogsMessage
    {
        public string Name { get; set; }
        public string LogsFolder { get; set; }
        public string LogFilenameFilter { get; set; }
    }
}
