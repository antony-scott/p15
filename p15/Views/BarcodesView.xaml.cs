using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace p15.Views
{
    public class BarcodesView : UserControl
    {
        public BarcodesView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
