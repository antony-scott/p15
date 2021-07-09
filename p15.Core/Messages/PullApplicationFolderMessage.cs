using System;

namespace p15.Core.Messages
{
    public class PullApplicationFolderMessage
    {
        public Guid ApplicationId { get; set; }
        public string PackageName { get; set; }
        public string ApplicationName { get; set; }
        public string Path { get; set; }
        public string Url { get; set; }
    }
}
