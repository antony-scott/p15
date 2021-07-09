using Dock.Model.Controls;

namespace p15.ViewModels
{
    public class MarkdownViewModel : Document
    {
        public string DocumentTitle { get; set; }
        public string Xaml { get; set; }
        public string Markdown { get; internal set; }
    }
}
