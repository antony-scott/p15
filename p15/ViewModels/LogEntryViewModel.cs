using Avalonia.Media;
using ReactiveUI;
using System;

namespace p15.ViewModels
{
    public class LogEntryViewModel : ViewModelBase
    {
        private bool _hover;
        private bool _isVisible;
        private string _toggleIndicatorText;

        public DateTime? Timestamp { get; set; }
        public string Severity { get; set; }
        public LogEntryViewModel Child { get; set; }
        public bool IsTopLevel { get; set; }
        public bool IsVisible { get => _isVisible; set => this.RaiseAndSetIfChanged(ref _isVisible, value); }
        public string Message { get; set; }
        public string ToggleIndicatorText { get => _toggleIndicatorText; set => this.RaiseAndSetIfChanged(ref _toggleIndicatorText, value); }
        public bool Hover
        {
            get => _hover;
            set
            {
                this.RaiseAndSetIfChanged(ref _hover, value);
                this.RaisePropertyChanged(nameof(Colour));
            }
        }

        public SolidColorBrush Colour => Severity?.ToLower() switch
        {
            "error" => new SolidColorBrush(Colors.IndianRed),
            "verbose" => new SolidColorBrush(Colors.Gray),
            "debug" => new SolidColorBrush(Colors.DimGray),
            "info" => new SolidColorBrush(Colors.LawnGreen),
            "information" => new SolidColorBrush(Colors.LawnGreen),
            "warning" => new SolidColorBrush(Colors.Orange),
            "fatal" => new SolidColorBrush(Colors.Red),
            "critical" => new SolidColorBrush(Colors.Red),
            _ => new SolidColorBrush(Colors.White)
        };

        public void ToggleMessage()
        {
            if (Child != null)
            {
                ToggleIndicatorText = ToggleIndicatorText == "+" ? "-" : "+";
                Child.IsVisible = !Child.IsVisible;
            }
        }
    }
}
