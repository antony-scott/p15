namespace p15
{
    public class RavenSettings
    {
        public int Version { get; set; }
        public string Location { get; set; }
        public int Port { get; set; }
    }

    public class ManagedRepository
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Folder { get; set; }
    }

    public class AppSettings
    {
        public RavenSettings[] RavenServers { get; set; }
        public string PackagesRepository { get; set; }
        public ManagedRepository[] ManagedRepositories { get; set; }
        public string[] StartupTasks { get; set; }
        public string Terminal { get; set; }
    }
}
