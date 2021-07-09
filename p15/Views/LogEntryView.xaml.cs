using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace p15.Views
{
    public class LogEntryView : UserControl
    {
        public LogEntryView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
