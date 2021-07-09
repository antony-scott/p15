namespace p15.Core.Models
{
    public class App
    {
        public string Name { get; set; }
        public string Project { get; set; }
        public string ApplicationType { get; set; }
        public string[] Arguments { get; set; }
        public string Source { get; set; }
        public bool RequiresExternalAccess { get; set; }
        public string[] Tags { get; set; }
        public string AnonymousAuthentication { get; set; }
        public string BasicAuthentication { get; set; }
        public string PackageName { get; internal set; }
        public string Logs { get; set; }
    }
}
