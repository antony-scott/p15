using Avalonia.Media;
using Dock.Model.Controls;
using DynamicData;
using Newtonsoft.Json;
using p15.Core.Extensions;
using p15.Core.Messages;
using p15.Core.Services;
using p15.Extensions;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace p15.ViewModels
{
    public class LogViewModel : Document, IActivatableViewModel
    {
        private int _uiScale;
        private int _fontSize;

        [JsonProperty]
        public string Name { get; set; } = null;
        [JsonIgnore]
        public int FontSize
        {
            get => _fontSize;
            set
            {
                this.RaiseAndSetIfChanged(ref _fontSize, value);
                this.RaisePropertyChanged(nameof(TimestampColumnWidth));
                this.RaisePropertyChanged(nameof(SeverityColumnWidth));
                this.RaisePropertyChanged(nameof(ToggleIndicatorColumnWidth));
            }
        }

        public int UiScale
        {
            get => _uiScale;
            set
            {
                this.RaiseAndSetIfChanged(ref _uiScale, value);
                FontSize = FontSizes.Text.Scale(_uiScale);
            }
        }

        public int TimestampColumnWidth => (int)(FontSize * 14);
        public int SeverityColumnWidth => (int)(FontSize * 8);
        public int ToggleIndicatorColumnWidth => FontSize;

        [JsonIgnore]
        public FontFamily FontFamily { get; set; } = new FontFamily("Consolas");

        public bool AutoScrollEnabled { get; set; } = true;

        IDisposable _logEntrySubscriber = null;

        public ObservableCollection<LogEntryViewModel> LogEntries { get; } = new ObservableCollection<LogEntryViewModel>();

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        public LogViewModel(
            string name,
            int uiScale,
            IMessagingService messagingService)
        {
            Name = name;
            UiScale = uiScale;

            messagingService
                .SubscribeOnUIThread<UiScaleChangedMessage>(msg =>
                {
                    UiScale = msg.UiScale;
                });

            this
                .WhenAnyValue(x => x.Name)
                //.WhenAnyValue(x => x.Filters)
                .Subscribe(name =>
                {
                    messagingService
                        .Subscribe<LogEntriesMessage>(msg =>
                        {
                            if (msg.Name != Name) return;
                            if (msg.LogEntries?.Any() ?? false)
                            {
                                var logEntriesObserver = msg
                                    .LogEntries
                                    .ToObservable()
                                    .ObserveOn(RxApp.MainThreadScheduler);

                                // TODO: this will change based on filters set in the UI (ie - debug only / message contains "x" / etc)
                                // logEntriesObserver = logEntriesObserver
                                //    .Where(x => x.Severity == "Debug");

                                _logEntrySubscriber?.Dispose();
                                _logEntrySubscriber = logEntriesObserver
                                    .Subscribe(entry =>
                                    {
                                        var lines = entry.Message.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                                        if (!lines.Any())
                                            return;

                                        var firstLine = lines.First();
                                        var childLines = lines.Skip(1).ToArray();

                                        var childEntry = childLines.Any()
                                            ? new LogEntryViewModel
                                            {
                                                Message = string.Join(Environment.NewLine, childLines),
                                                Severity = entry.Severity,
                                                Timestamp = null,
                                                IsVisible = false,
                                                IsTopLevel = false,
                                                Child = null,
                                                ToggleIndicatorText = ""
                                            }
                                            : null;

                                        var parent = new LogEntryViewModel
                                        {
                                            Message = firstLine,
                                            Severity = entry.Severity,
                                            Timestamp = entry.Timestamp,
                                            IsVisible = true,
                                            IsTopLevel = true,
                                            Child = childEntry,
                                            ToggleIndicatorText = childEntry != null ? "+" : ""
                                        };

                                        LogEntries.Add(parent);
                                        if (childEntry != null)
                                        {
                                            LogEntries.Add(childEntry);
                                        }

                                        LogEntries.Add(App.Mapper.Map<LogEntryViewModel>(entry));
                                    });
                            }
                        });
                });
        }

        public void Clear()
        {
            LogEntries.Clear();
        }
    }

}
