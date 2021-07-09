using System;
using System.IO;
using System.Linq;

namespace p15.Core.Services
{
    public class FilenameService
    {
        public string GetConfigFilename(string path)
        {
            var projectName = Path.GetFileName(path);
            var csprojFilename = Directory
                .GetFiles(path, $"{projectName}.csproj", SearchOption.AllDirectories)
                .FirstOrDefault();
            if (csprojFilename == null)
                return null;
            var csprojFolder = Path.GetDirectoryName(csprojFilename);
            var filenames = Directory
                .GetFiles(csprojFolder, "*.config", SearchOption.AllDirectories);
            var configFilename = filenames
                .FirstOrDefault(x => new[] { "web.config", "app.config" }.Contains(Path.GetFileName(x).ToLower()));
            return configFilename;
        }
    }
}
