using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace p15.Views
{
    public class MarkdownView : UserControl
    {
        public MarkdownView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
