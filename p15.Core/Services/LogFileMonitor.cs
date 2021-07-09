using p15.Core.Extensions;
using p15.Core.Models;
using p15.Core.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace p15.Core.Services
{
    public sealed class LogFileMonitor : IDisposable
    {
        private DateTime _lastWriteTime;
        private long? _previousSize;
        private readonly TraceService _traceService;
        private readonly StandardLogParser _logParser;

        public string Name { get; private set; }
        public Guid Identifier { get; private set; }
        public string LogFilename { get; private set; }
        public bool IsMonitoring { get; private set; }
        public FileSystemWatcher Watcher { get; private set; }

        public LogFileMonitor(
            TraceService traceService,
            StandardLogParser logParser)
        {
            _traceService = traceService;
            _logParser = logParser;
        }

        public LogFileMonitor WithName(string name)
        {
            Name = name;
            return this;
        }

        public LogFileMonitor WithIdentifier(Guid identifier)
        {
            Identifier = identifier;
            return this;
        }

        public LogFileMonitor Watch(string folder, string filter, Action<object, FileSystemEventArgs> changedOrCreatedAction)
        {
            if (Watcher == null)
            {
                LogFilename = folder.GetMostRecentFile(filter);
                _traceService.Info($"LogFileMonitor.Watch > logFilename = {LogFilename}");

                Watcher = new FileSystemWatcher
                {
                    Path = folder,
                    Filter = filter,
                    EnableRaisingEvents = true,
                    NotifyFilter = NotifyFilters.LastWrite
                };

                Watcher.Created += (sender, args) => changedOrCreatedAction(sender, args);
                Watcher.Changed += (sender, args) => changedOrCreatedAction(sender, args);

                IsMonitoring = true;
            }
            else
            {
                _traceService.Error($"LogFileMonitor.Watch > Watcher is already set! ({Watcher.Path} / {Watcher.Filter})");
            }

            return this;
        }

        public void Dispose()
        {
            Watcher?.Dispose();
        }

        private static FileInfo GetLogFile(string logFilename)
        {
            string folder;
            string searchPattern;

            if (Directory.Exists(logFilename))
            {
                folder = logFilename;
                searchPattern = "*.*";
            }
            else
            {
                folder = Path.GetDirectoryName(logFilename);
                searchPattern = Path.GetFileNameWithoutExtension(logFilename) + "*" + Path.GetExtension(logFilename);
            }

            if (string.IsNullOrWhiteSpace(folder)) return null;

            var logFolder = folder;

            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            var logfile =
                Directory.GetFiles(logFolder, searchPattern)
                    .Select(filename => new FileInfo(filename))
                    .OrderByDescending(x => x.LastWriteTime)
                    .FirstOrDefault();

            return logfile;
        }

        public bool HasNewLogData(FileInfo logFile)
        {
            if (logFile == null)
            {
                return true;
            }

            if (logFile.FullName != LogFilename)
            {
                return true;
            }

            if (logFile.LastWriteTime != _lastWriteTime)
            {
                return true;
            }

            if (logFile.Length != _previousSize)
            {
                return true;
            }

            return false;
        }

        public IEnumerable<LogEntryModel> GetNewLogEntries(string logfilename = null, int? lastNEntries = null)
        {
            var logfile = GetLogFile(logfilename ?? LogFilename);
            if (logfile == null) return null;
            if (!HasNewLogData(logfile)) return null;

            // get the new log file content
            using var fs = File.Open(logfile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var sr = new StreamReader(fs);

            if (logfile.Name == Path.GetFileName(LogFilename))
            {
                fs.Seek(_previousSize ?? 0, SeekOrigin.Begin);
            }

            var text = sr.ReadToEnd();
            var entries = _logParser.Parse(text, lastNEntries);

            _lastWriteTime = logfile.LastWriteTime;
            _previousSize = _previousSize ?? 0;
            _previousSize += text.Length;
            //_previousSize = logfile.Length; // TODO: place this "pointer" to the last "gap" line, so the partially received log entry is picked up when the rest of it is written to the file
            LogFilename = logfile.FullName;

            return entries;
        }
    }
}