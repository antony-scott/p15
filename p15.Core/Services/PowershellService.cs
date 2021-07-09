using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using p15.Core.Messages;

namespace p15.Core.Services
{
    public class PowershellService : IListen
    {
        private readonly TraceService _traceService;
        private readonly IMessagingService _messagingService;

        public PowershellService(
            TraceService traceService,
            IMessagingService messagingService)
        {
            _traceService = traceService;
            _messagingService = messagingService;
        }
        public void Listen()
        {
            _messagingService.Subscribe<PerformStartupTasksMessage>(msg => InstallPowershellModule());
        }

        private static string GetPowershellModulePath() =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Powershell",
                "Modules",
                "p15");

        private static void InstallPowershellModule()
        {
            var embeddedProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());

            var path = GetPowershellModulePath();

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            CopyEmbeddedFile(embeddedProvider, path, "p15.psm1");
        }

        private static void CopyEmbeddedFile(EmbeddedFileProvider embeddedProvider, string path, string filename)
        {
            using (var stream = embeddedProvider.GetFileInfo($"_PowershellModule.{filename}").CreateReadStream())
            using (var reader = new StreamReader(stream))
            {
                var contents = reader.ReadToEnd();
                File.WriteAllText(Path.Combine(path, filename), contents);
            }
        }

        private Collection<PSObject> ExecuteInternal(Action<PowerShell> ps)
        {
            var defaultSessionState = InitialSessionState.CreateDefault();
            defaultSessionState.ExecutionPolicy = Microsoft.PowerShell.ExecutionPolicy.Unrestricted;
            defaultSessionState.ImportPSModule(Path.Combine(GetPowershellModulePath(), "p15.psm1"));

            using (var session = PowerShell.Create(defaultSessionState))
            {
                ps(session);

                session.Streams.Debug.DataAdded += Debug_DataAdded;
                session.Streams.Information.DataAdded += Information_DataAdded;
                session.Streams.Warning.DataAdded += Warning_DataAdded;
                session.Streams.Error.DataAdded += Error_DataAdded;
                session.Streams.Progress.DataAdded += Progress_DataAdded;
                session.Streams.Verbose.DataAdded += Verbose_DataAdded;

                try
                {
                    var pipelineObjects = session.Invoke();
                    return pipelineObjects;
                }
                catch (Exception ex)
                {
                    _traceService.Error(ex.Message);
                    return null;
                }
            }
        }

        private async Task<PSDataCollection<PSObject>> ExecuteInternalAsync(Action<PowerShell> ps)
        {
            var defaultSessionState = InitialSessionState.CreateDefault();
            defaultSessionState.ExecutionPolicy = Microsoft.PowerShell.ExecutionPolicy.Unrestricted;
            defaultSessionState.ImportPSModule("p15");

            using (var session = PowerShell.Create(defaultSessionState))
            {
                ps(session);

                session.Streams.Debug.DataAdded += Debug_DataAdded;
                session.Streams.Information.DataAdded += Information_DataAdded;
                session.Streams.Warning.DataAdded += Warning_DataAdded;
                session.Streams.Error.DataAdded += Error_DataAdded;
                session.Streams.Progress.DataAdded += Progress_DataAdded;
                session.Streams.Verbose.DataAdded += Verbose_DataAdded;

                try
                {
                    var pipelineObjects = await session.InvokeAsync();
                    return pipelineObjects;
                }
                catch (Exception ex)
                {
                    _traceService.Error(ex.Message);
                    return null;
                }
            }
        }

        public void Execute(Action<PowerShell> ps)
        {
            var pipelineObjects = ExecuteInternal(ps);
            if (pipelineObjects != null)
            {
                foreach (var item in pipelineObjects)
                {
                    _traceService.Debug(item.BaseObject.ToString());
                }
            }
        }

        public T Execute<T>(Action<PowerShell> ps)
        {
            var pipelineObjects = ExecuteInternal(ps);
            var item = pipelineObjects.FirstOrDefault();
            if (item != null)
            {
                var result = JsonConvert.DeserializeObject<T>(item.BaseObject.ToString());
                return result;
            }
            return default(T);
        }

        public async Task ExecuteAsync(Action<PowerShell> ps)
        {
            var pipelineObjects = await ExecuteInternalAsync(ps);
            if (pipelineObjects != null)
            {
                foreach (var item in pipelineObjects)
                {
                    _traceService.Debug(item.BaseObject.ToString());
                }
            }
        }

        public async Task<T> ExecuteAsync<T>(Action<PowerShell> ps)
        {
            var pipelineObjects = await ExecuteInternalAsync(ps);
            var item = pipelineObjects.FirstOrDefault();
            if (item != null)
            {
                var result = JsonConvert.DeserializeObject<T>(item.BaseObject.ToString());
                return result;
            }
            return default(T);
        }

        private void Verbose_DataAdded(object sender, DataAddedEventArgs e)
        {
            var streamObjectReceived = sender as PSDataCollection<VerboseRecord>;
            if (streamObjectReceived != null)
            {
                var currentStreamRecord = streamObjectReceived[e.Index];
                _traceService.Debug($"PS> Verbose > {currentStreamRecord.Message}");
            }
        }

        private void Warning_DataAdded(object sender, DataAddedEventArgs e)
        {
            var streamObjectReceived = sender as PSDataCollection<WarningRecord>;
            if (streamObjectReceived != null)
            {
                var currentStreamRecord = streamObjectReceived[e.Index];
                _traceService.Debug($"PS> Warning > {currentStreamRecord.Message}");
            }
        }

        private void Information_DataAdded(object sender, DataAddedEventArgs e)
        {
            var streamObjectReceived = sender as PSDataCollection<InformationRecord>;
            if (streamObjectReceived != null)
            {
                var currentStreamRecord = streamObjectReceived[e.Index];
                _traceService.Debug($"PS> Information > {currentStreamRecord.MessageData}");
            }
        }

        private void Error_DataAdded(object sender, DataAddedEventArgs e)
        {
            var streamObjectReceived = sender as PSDataCollection<ErrorRecord>;
            if (streamObjectReceived != null)
            {
                var currentStreamRecord = streamObjectReceived[e.Index];
                _traceService.Debug($"PS> Information > {currentStreamRecord.ToString()}");
            }
        }

        private void Progress_DataAdded(object sender, DataAddedEventArgs e)
        {
            var streamObjectReceived = sender as PSDataCollection<ProgressRecord>;
            if (streamObjectReceived != null)
            {
                var currentStreamRecord = streamObjectReceived[e.Index];
                _traceService.Debug($"PS> Information > {currentStreamRecord.PercentComplete}%");
            }
        }

        private void Debug_DataAdded(object sender, DataAddedEventArgs e)
        {
            var streamObjectReceived = sender as PSDataCollection<DebugRecord>;
            if (streamObjectReceived != null)
            {
                var currentStreamRecord = streamObjectReceived[e.Index];
                _traceService.Debug($"PS> Information > {currentStreamRecord.Message}");
            }
        }
    }
}
