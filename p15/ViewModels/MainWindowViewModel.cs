using Dock.Model;
using p15.Core.Extensions;
using p15.Core.Messages;
using p15.Core.Models;
using p15.Core.Services;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace p15.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IMessagingService _messagingService;

        private IDock _layout;
        private int _uiScale;
        private int _fontSize;

        public IDock Layout
        {
            get => _layout;
            set => this.RaiseAndSetIfChanged(ref _layout, value);
        }

        public int FontSize
        {
            get => _fontSize;
            set => this.RaiseAndSetIfChanged(ref _fontSize, value);
        }


        public int UiScale
        {
            get => _uiScale;
            set
            {
                this.RaiseAndSetIfChanged(ref _uiScale, value);
                _messagingService.SendMessage(new UiScaleChangedMessage { UiScale = value });
                FontSize = FontSizes.MenuItem.Scale(_uiScale);
            }
        }

        public ObservableCollection<MenuItemViewModel> MenuItems { get; } = new ObservableCollection<MenuItemViewModel>();

        public MainWindowViewModel(
            p15Model p15Model,
            IMessagingService messagingService)
        {
            _messagingService = messagingService;

            UiScale = p15Model.UiScale;

            //this
            //    .WhenAnyValue(x => x.UiScale)
            //    .Subscribe(i =>
            //    {
            //        messagingService.SendMessage(new UiScaleChangedMessage { UiScale = i });
            //    });

            messagingService.Subscribe<LoadPackageMessage>(msg =>
            {
                var bookmarks = p15Model
                    .Bookmarks
                    .Where(x => x.PackageName == msg.PackageName)
                    .ToArray();

                if (!bookmarks.Any()) return;

                var bookmarksMenuItem = MenuItems.FirstOrDefault(x => x.Header == "Bookmarks");
                if (bookmarksMenuItem == null)
                {
                    bookmarksMenuItem = new MenuItemViewModel { Header = "Bookmarks" };
                    MenuItems.Add(bookmarksMenuItem);
                }

                var packageBookmarksMenuItem = bookmarksMenuItem.Items.FirstOrDefault(x => x.Header == msg.PackageName);
                if (packageBookmarksMenuItem == null)
                {
                    packageBookmarksMenuItem = new MenuItemViewModel { Header = msg.PackageName };
                    bookmarksMenuItem.Items.Add(packageBookmarksMenuItem);
                }

                foreach (var bookmark in bookmarks)
                {
                    packageBookmarksMenuItem.Items.Add(new MenuItemViewModel
                    {
                        Header = bookmark.Name,
                        Command = ReactiveCommand.Create(() =>
                        {
                            messagingService.SendMessage(new OpenWebPageMessage { Url = bookmark.Url });
                        })
                    });
                }

                var logs = p15Model
                    .Logs
                    .Where(x => x.PackageName == msg.PackageName)
                    .ToArray();

                foreach (var log in logs)
                {
                    var folder = Path.GetDirectoryName(log.Filename);
                    if (Directory.Exists(folder))
                    {
                        var logFilenameFilter = Path
                            .GetFileName(log.Filename)
                            .Replace("-log", "-log-*");

                        messagingService.SendMessage(new MonitorLogsMessage
                        {
                            Name = log.Name,
                            LogFilenameFilter = logFilenameFilter,
                            LogsFolder = folder
                        });
                    }
                }
            });
        }
    }
}
