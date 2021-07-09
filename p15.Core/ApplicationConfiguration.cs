using System;
using System.IO;

namespace p15.Core
{
    public interface IApplicationConfiguration
    {
        string RootFolder { get; }
        string OperatingSystem { get; }
    }

    public class ApplicationConfiguration : IApplicationConfiguration
    {
        public string RootFolder
        {
            get
            {
                var rootFolder = Environment.GetEnvironmentVariable("DevRootFolder", EnvironmentVariableTarget.User);
                if (string.IsNullOrWhiteSpace(rootFolder))
                {
                    var folders = new[] { "C:\\_Code", "C:\\Code" };
                    foreach (var folder in folders)
                    {
                        if (Directory.Exists(folder))
                        {
                            Environment.SetEnvironmentVariable("DevRootFolder", folder, EnvironmentVariableTarget.User);
                            break;
                        }
                    }
                }
                return Environment.GetEnvironmentVariable("DevRootFolder", EnvironmentVariableTarget.User);
            }
        }
        public string OperatingSystem => "Windows";
    }
}
