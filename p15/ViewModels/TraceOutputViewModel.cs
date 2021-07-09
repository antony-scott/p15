using System;
using System.Collections.ObjectModel;
using Dock.Model.Controls;
using p15.Core.Extensions;
using p15.Core.Messages;
using p15.Core.Models;
using p15.Core.Services;
using p15.Extensions;
using ReactiveUI;

namespace p15.ViewModels
{
    public class TraceOutputViewModel : Document
    {
        private int _uiScale;
        private int _fontSize;

        public ObservableCollection<TraceViewModel> Traces { get; } = new ObservableCollection<TraceViewModel>();

        public int FontSize
        {
            get => _fontSize;
            set
            {
                this.RaiseAndSetIfChanged(ref _fontSize, value);
                this.RaisePropertyChanged(nameof(Height));
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

        public int Height => (int)(FontSize * 1.4);

        public TraceOutputViewModel(
            p15Model p15Model,
            IMessagingService messagingService)
        {
            Title = "Trace";
            UiScale = p15Model.UiScale;

            messagingService
                .SubscribeOnUIThread<UiScaleChangedMessage>(msg =>
                {
                    UiScale = msg.UiScale;
                });

            messagingService
                .SubscribeOnUIThread<TraceOutputMessage>(msg =>
                {
                    Traces.Add(new TraceViewModel
                    {
                        Level = msg.Level,
                        Message = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff} -> {msg.Level} -> {msg.Trace}"
                    });
                });
        }

        public void Clear()
        {
            Traces.Clear();
        }
    }
}
