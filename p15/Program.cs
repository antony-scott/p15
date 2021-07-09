using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace p15
{
    class Program
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            using (var mutex = new Mutex(true, "p15-client", out var createdNew))
            {
                if (createdNew)
                {
                    BuildAvaloniaApp()
                        .StartWithClassicDesktopLifetime(args);
                }
                else
                {
                    var current = Process.GetCurrentProcess();
                    foreach (var process in Process.GetProcessesByName(current.ProcessName))
                    {
                        if (process.Id != current.Id)
                        {
                            SetForegroundWindow(process.MainWindowHandle);
                            break;
                        }
                    }
                }
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp() => AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
    }
}
