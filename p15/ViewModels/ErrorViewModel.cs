using ReactiveUI;
using System;

namespace p15.ViewModels
{
    public class ErrorViewModel : ReactiveObject
    {
        public string ApplicationName { get; set; }
        public DateTime Timestamp { get; set; }
        public string Error { get; set; }
    }
}
