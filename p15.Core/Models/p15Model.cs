using System.Collections.Generic;
using System.Collections.ObjectModel;
using p15.Core.Messages;
using p15.Core.Services;
using p15.Extensions;

namespace p15.Core.Models
{
    public class p15Model
    {
        public int UiScale { get; set; }
        public ObservableCollection<string> PackageNames { get; } = new ObservableCollection<string>();
        public List<App> Applications { get; } = new List<App>();
        public List<Barcode> Barcodes { get; } = new List<Barcode>();
        public List<Intent> Intents { get; } = new List<Intent>();
        public List<Bookmark> Bookmarks { get; } = new List<Bookmark>();
        public List<Log> Logs { get; } = new List<Log>();
        public List<FirewallRule> FirewallRules { get; } = new List<FirewallRule>();

        public p15Model(IMessagingService messagingService)
        {
            UiScale = 100;
            messagingService
                .SubscribeOnUIThread<UiScaleChangedMessage>(msg =>
                {
                    UiScale = msg.UiScale;
                });
        }
    }
}
