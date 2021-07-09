using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace p15.Views
{
    public class BarcodeView : UserControl
    {
        public BarcodeView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
