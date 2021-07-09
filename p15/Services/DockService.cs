using Microsoft.Extensions.DependencyInjection;
using Dock.Model;
using p15.Core.Messages;
using p15.Core.Models;
using p15.Core.Services;
using p15.Extensions;
using p15.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace p15.Services
{
    public class DockService : IListen
    {
        private IMessagingService _messagingService;
        private ILogFileMonitorService _logFileMonitorService;
        private IFactory _factory;
        private IDock _dock;
        private MainViewModel _mainViewModel;
        private p15Model _p15Model;
        private readonly TraceService _traceService;
        private readonly ProcessService _processService;
        private readonly ClipboardService _clipboardService;

        public DockService(
            IMessagingService messagingService,
            ILogFileMonitorService logFileMonitorService,
            IFactory factory,
            IDock dock,
            MainViewModel mainViewModel,
            p15Model p15Model,
            TraceService traceService,
            ProcessService processService,
            ClipboardService clipboardService)
        {
            _messagingService = messagingService;
            _logFileMonitorService = logFileMonitorService;
            _factory = factory;
            _dock = dock;
            _mainViewModel = mainViewModel;
            _p15Model = p15Model;
            _traceService = traceService;
            _processService = processService;
            _clipboardService = clipboardService;
        }

        public void Listen()
        {
            _messagingService
                .SubscribeOnUIThread<LoadPackageMessage>(async msg =>
                {
                    var title = $"{msg.PackageName} Applications";
                    var document = FindDocument(title);
                    if (document == null)
                    {
                        var appsViewModel = await CreateAppsViewModel(msg.PackageName, title);
                        document = appsViewModel;
                        _mainViewModel.DefaultToolDock
                            .VisibleDockables
                            .Add(document);
                        _mainViewModel.DefaultToolDock.ActiveDockable = document;
                    }
                    _factory.SetActiveDockable(document);
                });

            _messagingService
                .SubscribeOnUIThread<LoadPackageMessage>(msg =>
                {
                    var title = $"{msg.PackageName} Barcodes";
                    var document = FindDocument(title);
                    if (document == null)
                    {
                        var barcodesViewModel = CreateBarcodesViewModel(msg.PackageName, title);
                        if (barcodesViewModel != null)
                        {
                            document = barcodesViewModel;
                            _mainViewModel.DefaultToolDock
                                .VisibleDockables
                                .Add(document);
                        }
                    }
                });

            _messagingService
                .SubscribeOnUIThread<ViewMarkdownMessage>(msg =>
                {
                    var markdown = File.ReadAllText(msg.Filename);
                    var xaml = Markdig.Wpf.Markdown.ToXaml(markdown);
                    var title = $"{msg.Name} readme";
                    var document = FindDocument(title);
                    if (document == null)
                    {
                        document = new MarkdownViewModel
                        {
                            DocumentTitle = title,
                            Markdown = markdown,
                            Xaml = xaml
                        };
                        _mainViewModel.DefaultPane.VisibleDockables.Add(document);
                        _mainViewModel.DefaultPane.ActiveDockable = document;
                    }
                    _factory.SetActiveDockable(document);
                });

            _messagingService
                .SubscribeOnUIThread<MonitorLogsMessage>(msg =>
                {
                    if (msg?.LogsFolder != null && !Directory.Exists(msg.LogsFolder))
                    {
                        Directory.CreateDirectory(msg.LogsFolder);
                    }

                    var id = _logFileMonitorService.AddMonitor(msg.Name, msg.LogsFolder, msg.LogFilenameFilter);

                    var title = $"{msg.Name} Logs";
                    var document = FindDocument(title);
                    if (document == null)
                    {
                        document = new LogViewModel(msg.Name, _p15Model.UiScale, _messagingService)
                        {
                            Id = id.ToString(),
                            Title = title
                        };
                        _mainViewModel.DefaultPane.VisibleDockables.Add(document);
                        _mainViewModel.DefaultPane.ActiveDockable = document;
                        _logFileMonitorService.GetPreviousLogs(id, 100);
                    }
                    _factory.SetActiveDockable(document);
                });

            _messagingService
                .SubscribeOnUIThread<MonitorCustomLogsMessage>(msg =>
                {
                    var title = $"{msg.Name} Logs";
                    var document = FindDocument(title);
                    if (document == null)
                    {
                        document = new LogViewModel(msg.Name, _p15Model.UiScale, _messagingService)
                        {
                            Id = msg.Id.ToString(),
                            Title = title
                        };
                        _mainViewModel.DefaultPane.VisibleDockables.Add(document);
                        _mainViewModel.DefaultPane.ActiveDockable = document;
                    }
                    _factory.SetActiveDockable(document);
                });

            _messagingService
                .SubscribeOnUIThread<ShowViewMessage>(msg =>
                {
                    switch (msg.View.ToLower())
                    {
                        case "errors":
                            var document = FindDocument(msg.View);
                            if (document == null)
                            {
                                document = new ErrorsViewModel(_messagingService, _clipboardService)
                                {
                                    Id = msg.View,
                                    Title = msg.View
                                };
                                _mainViewModel.DefaultPane.VisibleDockables.Add(document);
                                _mainViewModel.DefaultPane.ActiveDockable = document;
                            }
                            _factory.SetActiveDockable(document);
                            break;
                    }
                });
        }

        private IDockable FindDocument(string title)
        {
            var document = _factory.FindDockable(_dock, x => x.Title == title);
            return document;
        }

        private BarcodesViewModel CreateBarcodesViewModel(string packageName, string title)
        {
            var viewModel = App.Services.GetService<BarcodesViewModel>();
            viewModel.PackageName = packageName;
            viewModel.Title = title;
            viewModel.Barcodes = new ObservableCollection<BarcodeViewModel>(_p15Model
                .Barcodes
                .Where(barcode => barcode.PackageName == packageName)
                .Select(barcode =>
                {
                    var vm = App.Services.GetService<BarcodeViewModel>();
                    App.Mapper.Map(barcode, vm);
                    return vm;
                }));
            return viewModel.Barcodes.Any() ? viewModel : null;
        }

        private async Task<AppsViewModel> CreateAppsViewModel(string packageName, string title)
        {
            var viewModel = new AppsViewModel(_p15Model.UiScale, _messagingService)
            {
                PackageName = packageName,
                Title = title,
                Applications = new ObservableCollection<AppViewModel>(_p15Model
                    .Applications
                    .Where(app => app.PackageName == packageName)
                    .Select(app =>
                    {
                        var vm = App.Services.GetService<AppViewModel>();
                        App.Mapper.Map(app, vm);
                        return vm;
                    })
                )
            };

            foreach (var app in viewModel.Applications)
            {
                _messagingService.SendMessage(new PrepareApplicationMessage
                {
                    ApplicationId = app.ApplicationId,
                    ApplicationName = app.Name,
                    ProjectName = app.Project
                });
            }

            _messagingService
                .SubscribeOnUIThread<ApplicationPollMessage>(async msg =>
                {
                    await CheckApplications(_messagingService, viewModel);
                });

            await CheckApplications(_messagingService, viewModel);

            return viewModel;
        }

        private async Task CheckApplications(IMessagingService messagingService, AppsViewModel viewModel)
        {
            foreach (var app in viewModel.Applications)
            {
                // check if process died
                if (app.ProcessId.HasValue)
                {
                    var processIsRunning = await _processService.IsProcessRunning(app.ProcessId.Value);
                    if (!processIsRunning)
                    {
                        _traceService.Warn($"{app.Name} has stopped");
                        app.ProcessId = null;
                    }
                }
                else
                {
                    // check if process has been started externally
                    messagingService.SendMessage(new CheckIfApplicationIsRunningMessage
                    {
                        ApplicationId = app.ApplicationId,
                        Path = app.Path,
                        ApplicationType = app.ApplicationType,
                        Project = app.Project
                    });
                }
            }
        }
    }
}
