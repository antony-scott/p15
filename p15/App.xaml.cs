using AutoMapper;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Dock.Model;
using Material.Icons;
using Material.Icons.Avalonia;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using p15.Core;
using p15.Core.Messages;
using p15.Core.Models;
using p15.Core.Parsers;
using p15.Core.Services;
using p15.Plumbing;
using p15.Services;
using p15.ViewModels;
using p15.Views;
using Quartz;
using Quartz.Impl;
using ReactiveUI;
using Splat;
using System;
using System.IO;
using System.Threading.Tasks;

namespace p15
{
    public class App : Application
    {
        public static ServiceProvider Services { get; private set; }
        public static IMapper Mapper { get; private set; }

        private IScheduler _scheduler;
        private IServiceCollection _services;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void InitialiseListeners()
        {
            if (Avalonia.Controls.Design.IsDesignMode) return;

            var listenerTypes = new[]
            {
                typeof(AppService),
                typeof(PackageService),
                typeof(FileSystemService),
                typeof(ProcessService),
                typeof(DockService),
                typeof(BarcodeService),
                typeof(NetworkService),
                typeof(RavenService),
                typeof(SyncService),
                typeof(PowershellService)
            };

            foreach (var listenerType in listenerTypes)
            {
                (Services.GetService(listenerType) as IListen)?.Listen();
            }
        }

        public override void RegisterServices()
        {
            base.RegisterServices();

            _services = new ServiceCollection()
                .AddSingleton<IApplicationConfiguration, ApplicationConfiguration>()
                .AddSingleton<IMessagingService, MessagingService>()
                .AddSingleton<TraceOutputViewModel>()
                .AddSingleton<MainViewModel>()
                .AddTransient<AppsViewModel>()
                .AddTransient<AppViewModel>()
                .AddTransient<BarcodeViewModel>()
                .AddTransient<BarcodesViewModel>()
                .AddSingleton<p15Model>()
                .AddSingleton<StartViewModel>()
                .AddSingleton<MainWindowViewModel>()
                .AddSingleton<AppService>()
                .AddSingleton<RavenService>()
                .AddSingleton<ILogFileMonitorService, LogFileMonitorService>()
                .AddSingleton<FilenameService>()
                .AddSingleton<ProcessService>()
                .AddSingleton<PackageService>()
                .AddSingleton<TraceService>()
                .AddSingleton<FileSystemService>()
                .AddSingleton<DockService>()
                .AddSingleton<BarcodeService>()
                .AddSingleton<NetworkService>()
                .AddSingleton<AdbLogParser>()
                .AddSingleton<StandardLogParser>()
                .AddSingleton<IDateTimeService, DateTimeService>()
                .AddSingleton<TextReplacementService>()
                .AddSingleton<IFactory, DockFactory>()
                .AddSingleton<PowershellService>()
                .AddSingleton<ClipboardService>()
                .AddSingleton<MsmqService>()
                .AddSingleton<SyncService>()
                .AddSingleton<BuildService>()
                .AddSingleton<FirewallService>()
                .AddSingleton<IDock>(serviceProvider =>
                {
                    var factory = serviceProvider.GetService<IFactory>();
                    var layout = factory.CreateLayout();
                    return layout;
                })
                .AddTransient<LogFileMonitor>()
                ;

            Mapper = Mapping.Configure();
            var config = LoadConfiguration();
            if (config != null)
            {
                _services.AddOptions<AppSettings>().Bind(config);
            }

            Services = _services.BuildServiceProvider();

            InitialiseListeners();
        }

        private IConfigurationSection LoadConfiguration()
        {
            if (Avalonia.Controls.Design.IsDesignMode) return null;

            var basePath = Directory.GetCurrentDirectory();
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("p15.config.json", optional: false, reloadOnChange: true);
            
            var userSettingsFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".p15config");
            if (File.Exists(userSettingsFilename))
            {
                var userFilename = "p15.config.user.json";
                var destFilename = Path.Combine(basePath, userFilename);
                File.Copy(userSettingsFilename, destFilename, overwrite: true);
                configBuilder = configBuilder.AddJsonFile(userFilename, optional: true, reloadOnChange: true);
            }

            var config = configBuilder.Build();
            var settings = config.GetSection("AppSettings");
            return settings;
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var factory = Services.GetService<IFactory>();
            var layout = Services.GetService<IDock>();
            var messagingService = Services.GetService<IMessagingService>();
            var mainWindowViewModel = Services.GetService<MainWindowViewModel>();

            factory.InitLayout(layout);

            mainWindowViewModel.Layout = layout;

            var fileMenu = new MenuItemViewModel { Header = "_File" };

            fileMenu.Items.Add(new MenuItemViewModel
            {
                Header = "Open _Root Folder",
                Command = ReactiveCommand.Create(() =>
                {
                    var applicationConfiguration = Services.GetService<IApplicationConfiguration>();
                    messagingService.SendMessage(new OpenFolderMessage
                    {
                        Folder = applicationConfiguration.RootFolder
                    });
                }),
                Icon = new MaterialIcon { Kind = MaterialIconKind.Folder }
            });

            fileMenu.Items.Add(new MenuItemViewModel
            {
                Header = "Open _Packages Folder",
                Command = ReactiveCommand.Create(() =>
                {
                    var applicationConfiguration = Services.GetService<IApplicationConfiguration>();
                    messagingService.SendMessage(new OpenFolderMessage
                    {
                        Folder = Path.Combine(applicationConfiguration.RootFolder, FileSystemService.PackagesFolderName)
                    });
                }),
                Icon = new MaterialIcon { Kind = MaterialIconKind.Folder }
            });

            fileMenu.Items.Add(new MenuItemViewModel
            {
                Header = "E_xit",
                Command = ReactiveCommand.Create(() =>
                {
                    ShutDown();
                    Environment.Exit(0);
                })  ,
                Icon = new MaterialIcon { Kind = MaterialIconKind.ExitRun }
            });

            mainWindowViewModel.MenuItems.Add(fileMenu);

            var viewMenu = new MenuItemViewModel { Header = "_View" };
            viewMenu.Items.Add(new MenuItemViewModel
            {
                Header = "_Errors",
                Command = ReactiveCommand.Create(() => messagingService.SendMessage(new ShowViewMessage { View = "Errors" })),
                Icon = new MaterialIcon { Kind = MaterialIconKind.Error }
            });
            mainWindowViewModel.MenuItems.Add(viewMenu);
                
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                var mainWindow = new MainWindow
                {
                    DataContext = mainWindowViewModel
                };

                mainWindow.Closing += (sender, e) =>
                {
                    if (layout is IDock dock)
                    {
                        dock.Close();
                        ShutDown();
                    }
                    _scheduler?.Shutdown();
                };

                desktopLifetime.MainWindow = mainWindow;

                desktopLifetime.Exit += (sender, e) =>
                {
                    if (layout is IDock dock)
                    {
                        dock.Close();
                        ShutDown();
                    }

                    _scheduler?.Shutdown();
                };
            }

            ScheduleJobs().GetAwaiter().GetResult();

            base.OnFrameworkInitializationCompleted();
        }

        private void ShutDown()
        {
            Services.GetService<RavenService>()?.StopAll();
            Services.GetService<ProcessService>()?.KillManagedProcesses();
        }

        private async Task ScheduleJobs()
        {
            _scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await _scheduler.Start();
            await SchedulePollJob<ApplicationPollMessage>("Running applications check");
            await SchedulePollJob<HealthCheckPollMessage>("Health check");
            await ScheduleStartupTasks();

            // TODO: subscribe different bits of the system to the HealthCheckPollMessage
            //       - each file monitor could subscribe and check for itself whether it's own
            //         FileSystemMonitor is still working, if not then it would restart it or
            //         recreate it as necessary
        }

        private async Task SchedulePollJob<T>(string identity) where T : IPollMessage, new()
        {
            var job = JobBuilder.Create<PollJob<T>>().Build();
            var trigger = TriggerBuilder
                .Create()
                .WithIdentity(identity)
                .WithPriority(1)
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithInterval(new T().Interval)
                    .RepeatForever())
                .Build();
            await _scheduler.ScheduleJob(job, trigger);
        }

        private class PollJob<T> : IJob where T : IPollMessage, new()
        {
            public Task Execute(IJobExecutionContext context)
            {
                var messagingService = Services.GetService<IMessagingService>();
                messagingService?.SendMessage(new T());
                return Task.CompletedTask;
            }
        }

        private async Task ScheduleStartupTasks()
        {
            var job = JobBuilder.Create<StartupTasksJob>().Build();
            var trigger = TriggerBuilder
                .Create()
                .WithIdentity("Startup Tasks")
                .WithPriority(1)
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithInterval(TimeSpan.FromSeconds(2))
                    .WithRepeatCount(0))
                .Build();
            await _scheduler.ScheduleJob(job, trigger);
        }

        private class StartupTasksJob : IJob
        {
            public Task Execute(IJobExecutionContext context)
            {
                var messagingService = Services.GetService<IMessagingService>();
                messagingService?.SendMessage(new PerformStartupTasksMessage());
                return Task.CompletedTask;
            }
        }
    }
}
