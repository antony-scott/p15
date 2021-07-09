using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace p15.Views
{
    public class StartView : UserControl
    {
        public StartView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
