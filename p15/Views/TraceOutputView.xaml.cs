using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.ReactiveUI;
using p15.ViewModels;
using ReactiveUI;
using System.Linq;

namespace p15.Views
{
    public class TraceOutputView : ReactiveUserControl<TraceOutputViewModel>
    {
        private readonly ListBox _listbox;

        public TraceOutputView()
        {
            AvaloniaXamlLoader.Load(this);

            _listbox = this.FindControl<ListBox>("Traces");
            this.WhenActivated(disposables =>
            {
                var itemsChanged = _listbox
                    .WhenAnyValue(lb => lb.ItemCount)
                    .Where(x => x > 0)
                    .Throttle(TimeSpan.FromMilliseconds(10))
                    .Select(x => x - 1)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(index =>
                    {
                        var item = ViewModel.Traces.ElementAt(index);
                        if (item != null)
                        {
                            _listbox.ScrollIntoView(item);
                        }
                    })
                    .DisposeWith(disposables);
            });

        }
    }
}
