using System.Collections.ObjectModel;
using Dock.Model.Controls;
using p15.Core.Extensions;
using p15.Core.Messages;
using p15.Core.Services;
using p15.Extensions;
using ReactiveUI;

namespace p15.ViewModels
{
    public class AppsViewModel : Document
    {
        private int _packageNameFontSize;
        private int _uiScale;
        private int _textFontSize;
        private int _lozengeWidth;
        private int _processInfoWidth;

        public ObservableCollection<AppViewModel> Applications { get; set; }

        public string PackageName { get; set; }
        
        public int MediumTextFontSize
        {
            get => _packageNameFontSize;
            set => this.RaiseAndSetIfChanged(ref _packageNameFontSize, value);
        }

        public int TextFontSize
        {
            get => _textFontSize;
            set => this.RaiseAndSetIfChanged(ref _textFontSize, value);
        }

        public int LozengeWidth
        {
            get => _lozengeWidth;
            set => this.RaiseAndSetIfChanged(ref _lozengeWidth, value);
        }

        public int ProcessInfoWidth
        {
            get => _processInfoWidth;
            set => this.RaiseAndSetIfChanged(ref _processInfoWidth, value);
        }

        public int UiScale
        {
            get => _uiScale;
            set
            {
                this.RaiseAndSetIfChanged(ref _uiScale, value);
                MediumTextFontSize = FontSizes.MediumText.Scale(_uiScale);
                TextFontSize = FontSizes.Text.Scale(_uiScale);
                LozengeWidth = 250.Scale(_uiScale);
                ProcessInfoWidth = 40.Scale(_uiScale);
            }
        }

        public AppsViewModel(
            int uiScale,
            IMessagingService messagingService)
        {
            UiScale = uiScale;

            messagingService
                .SubscribeOnUIThread<UiScaleChangedMessage>(msg =>
                {
                    UiScale = msg.UiScale;
                });
        }
    }
}
