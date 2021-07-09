using p15.Core.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace p15.Core.Services
{
    public interface ILogFileMonitorService
    {
        Guid AddMonitor(string name, string folder, string filter);
        void GetPreviousLogs(int numberOfLines);
        void GetPreviousLogs(Guid identifier, int numberOfLines);
    }

    public sealed class LogFileMonitorService : ILogFileMonitorService, IDisposable
    {
        private readonly List<LogFileMonitor> _monitors = new List<LogFileMonitor>();
        private readonly IMessagingService _messagingService;
        private readonly TraceService _traceService;
        private readonly IServiceProvider _services;

        public LogFileMonitorService(
            IMessagingService messagingService,
            TraceService traceService,
            IServiceProvider services)
        {
            _messagingService = messagingService;
            _traceService = traceService;
            _services = services;
        }

        public void Dispose()
        {
            foreach (var monitor in _monitors)
            {
                monitor.Dispose();
            }
            _monitors.Clear();
        }

        public Guid AddMonitor(string name, string folder, string filter)
        {
            var existingMonitor = _monitors.FirstOrDefault(x => x.Name == name);
            if (existingMonitor != null) return existingMonitor.Identifier;

            var identifier = Guid.NewGuid();
            var monitor = _services
                .GetService<LogFileMonitor>()
                .WithName(name)
                .WithIdentifier(identifier)
                .Watch(folder, filter, WatcherOnChangedOrCreated);

            _monitors.Add(monitor);
            return identifier;
        }

        public void GetPreviousLogs(int numberOfLines)
        {
            _monitors.ForEach(monitor => PublishMessage(monitor, lastNEntries: numberOfLines));
        }

        public void GetPreviousLogs(Guid identifier, int numberOfLines)
        {
            var monitor = _monitors.FirstOrDefault(m => m.Identifier == identifier);
            if (monitor != null)
            {
                PublishMessage(monitor, lastNEntries: numberOfLines);
            }
        }

        private void WatcherOnChangedOrCreated(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            var watcher = sender as FileSystemWatcher;
            var monitor = _monitors.FirstOrDefault(x => x.Watcher == watcher);
            if (monitor == null)
            {
                _traceService.Error($"Cannot find FileSystemWatcher for '{fileSystemEventArgs.FullPath}'");
                return;
            }

            PublishMessage(monitor, filename: fileSystemEventArgs.FullPath);
        }

        private void PublishMessage(LogFileMonitor monitor, string filename = null, int? lastNEntries = null)
        {
            var logEntries = monitor.GetNewLogEntries(logfilename: filename, lastNEntries: lastNEntries);

            if (logEntries?.Any() ?? false)
            {
                _messagingService
                    .SendMessage(new LogEntriesMessage
                    {
                        Name = monitor.Name,
                        Identifier = monitor.Identifier,
                        LogEntries = logEntries
                    });
            }
        }
    }
}
