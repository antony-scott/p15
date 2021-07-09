using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace p15.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
