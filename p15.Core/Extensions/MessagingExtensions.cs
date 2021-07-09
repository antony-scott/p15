using Avalonia.Threading;
using p15.Core.Services;
using System;

namespace p15.Extensions
{
    public static class MessagingExtensions
    {
        public static void SubscribeOnUIThread<T>(this IMessagingService messagingService, Action<T> action) where T : new()
        {
            messagingService.Subscribe<T>(async msg =>
            {
                await Dispatcher.UIThread.InvokeAsync(() => action(msg));
            });
        }
    }
}
