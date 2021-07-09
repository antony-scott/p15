using System.IO;
using System.Linq;

namespace p15.Core.Extensions
{
    public static class FileSystemExtensions
    {
        public static string GetContainingFolder(this string filename)
        {
            return !string.IsNullOrWhiteSpace(filename)
                ? System.IO.Path.GetDirectoryName(filename)
                : null;
        }

        public static string GetMostRecentFile(this string folder, string filter)
        {
            var filename = Directory
                .GetFiles(folder, filter)
                .Select(x => new FileInfo(x))
                .OrderByDescending(x => x.LastWriteTimeUtc)
                .Select(x => x.FullName)
                .FirstOrDefault();
            return filename;
        }
    }
}
