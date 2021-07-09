using Avalonia.Media;

namespace p15.ViewModels
{
    public class TraceViewModel : ViewModelBase
    {
        public string Level { get; set; }

        public string Message { get; set; }

        public SolidColorBrush Colour => Level?.ToLower() switch
        {
            "error" => new SolidColorBrush(Colors.IndianRed),
            "debug" => new SolidColorBrush(Colors.DimGray),
            "info" => new SolidColorBrush(Colors.LawnGreen),
            "information" => new SolidColorBrush(Colors.LawnGreen),
            "warning" => new SolidColorBrush(Colors.Orange),
            "warn" => new SolidColorBrush(Colors.Orange),
            _ => new SolidColorBrush(Colors.White)
        };
    }
}
