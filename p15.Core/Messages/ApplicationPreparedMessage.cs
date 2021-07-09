using System;

namespace p15.Core.Messages
{
    public class ApplicationPreparedMessage
    {
        public Guid ApplicationId { get; set; }
        public string Path { get; set; }
        public bool ProjectFolderExists { get; set; }
        public bool HasLogging { get; set; }
        public string LogsFolder { get; set; }
        public string LogFilenameFilter { get; set; }
    }
}
