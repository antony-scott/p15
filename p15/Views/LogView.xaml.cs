using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using p15.ViewModels;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace p15.Views
{
    public class LogView : ReactiveUserControl<LogViewModel>
    {
        private ListBox _listbox;

        public LogView()
        {
            this.InitializeComponent();

            _listbox = this.FindControl<ListBox>("LogEntries");
            this.WhenActivated(disposables =>
            {
                if (_listbox.ItemCount > 0)
                {
                    var lastItem = ViewModel.LogEntries.Last();
                    if (lastItem != null)
                    {
                        _listbox.ScrollIntoView(lastItem);
                    }
                }

                _listbox
                    .WhenAnyValue(lb => lb.ItemCount)
                    .Where(x => x > 0)
                    .Throttle(TimeSpan.FromMilliseconds(50))
                    .Select(x => x - 1)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(index =>
                    {
                        if (!ViewModel.AutoScrollEnabled) return;

                        var item = ViewModel.LogEntries.ElementAt(index);
                        if (item != null)
                        {
                            _listbox.SelectedIndex = -1;
                            _listbox.ScrollIntoView(item);
                        }
                    })
                    .DisposeWith(disposables);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
