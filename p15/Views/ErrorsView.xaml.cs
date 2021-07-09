using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace p15.Views
{
    public class ErrorsView : UserControl
    {
        public ErrorsView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
