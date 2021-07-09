using Dock.Model.Controls;
using p15.Core.Messages;
using p15.Core.Models;
using p15.Core.Services;
using p15.Extensions;
using ReactiveUI;
using System.Collections.Generic;

namespace p15.ViewModels
{
    public class StartViewModel : Document
    {
        private readonly IMessagingService _messagingService;
        private bool _isLoadingMessageVisible;
        private int _uiScale;
        private int _fontSize;
        private int _loadingThrobberFontSize;
        private int _packageButtonFontSize;

        public int FontSize
        {
            get => _fontSize;
            set => this.RaiseAndSetIfChanged(ref _fontSize, value);
        }

        public int PackageButtonFontSize
        {
            get => _packageButtonFontSize;
            set => this.RaiseAndSetIfChanged(ref _packageButtonFontSize, value);
        }

        public int LoadingThrobberFontSize
        {
            get => _loadingThrobberFontSize;
            set => this.RaiseAndSetIfChanged(ref _loadingThrobberFontSize, value);
        }

        public IEnumerable<string> PackageNames { get; }

        public bool IsLoadingMessageVisible
        {
            get => _isLoadingMessageVisible;
            set => this.RaiseAndSetIfChanged(ref _isLoadingMessageVisible, value);
        }

        public StartViewModel(
            p15Model model,
            IMessagingService messagingService)
        {
            _messagingService = messagingService;

            PackageNames = model.PackageNames;
            Title = "Start";

            FontSize = 10;
            LoadingThrobberFontSize = 50;
            PackageButtonFontSize = 20;

            IsLoadingMessageVisible = true;
            model.PackageNames.CollectionChanged += PackageNames_CollectionChanged;

            messagingService
                .SubscribeOnUIThread<UiScaleChangedMessage>(msg =>
                {
                    _uiScale = msg.UiScale;
                    FontSize = (int)(10 * (_uiScale / 100.0));
                    LoadingThrobberFontSize = (int)(50 * (_uiScale / 100.0));
                    PackageButtonFontSize = (int)(20 * (_uiScale / 100.0));
                });
        }

        private void PackageNames_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems.Count > 0)
            {
                IsLoadingMessageVisible = false;
            }
        }

        public void LoadPackage(string packageName)
        {
            _messagingService.SendMessage(new LoadPackageMessage { PackageName = packageName });
        }

        public override bool OnClose()
        {
            return false;
        }
    }
}
