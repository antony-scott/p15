using Dock.Model.Controls;
using p15.Core.Messages;
using p15.Core.Services;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace p15.ViewModels
{
    public class ErrorsViewModel : Document, IReactiveObject
    {
        IDisposable _errorSubscriber = null;

        private readonly ClipboardService _clipboardService;

        public ObservableCollection<ErrorViewModel> Errors { get; } = new ObservableCollection<ErrorViewModel>();

        public ErrorsViewModel(
            IMessagingService messagingService,
            ClipboardService clipboardService)
        {
            messagingService
                .Subscribe<LogEntriesMessage>(msg =>
                {
                    var errorsObserver = msg
                        .LogEntries
                        .ToObservable()
                        .Where(x => new[] { "error", "fatal" }.Contains(x.Severity?.ToLower() ?? ""))
                        .ObserveOn(RxApp.MainThreadScheduler);

                    _errorSubscriber?.Dispose();
                    _errorSubscriber = errorsObserver.Subscribe(error => Errors
                        .Add(new ErrorViewModel
                        {
                            ApplicationName = msg.Name,
                            Timestamp = error.Timestamp ?? DateTime.Now,
                            Error = error.Message
                        }));
                });
            _clipboardService = clipboardService;
        }

        public void ClearErrors()
        {
            Errors.Clear();
        }

        public void CopyToClipboard(string text)
        {
            _clipboardService.CopyToCliboard(text);
        }
    }
}
