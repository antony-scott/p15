using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace p15.Core.Builders
{
    public class ProcessBuilder
    {
        private ProcessStartInfo _processStartInfo;
        private Process _process;

        public ProcessBuilder(string filename)
        {
            _processStartInfo = new ProcessStartInfo();
            _process = new Process { StartInfo = _processStartInfo };

            _processStartInfo.FileName = filename;
            _processStartInfo.WorkingDirectory = Path.GetDirectoryName(filename);
            _processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        }

        public ProcessBuilder WithArguments(string arguments)
        {
            _processStartInfo.Arguments = arguments;
            return this;
        }

        public ProcessBuilder WithVisibleWindow()
        {
            _processStartInfo.WindowStyle = ProcessWindowStyle.Normal;
            return this;
        }

        public ProcessBuilder WithMinimisedWindow()
        {
            _processStartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            return this;
        }

        public ProcessBuilder Elevated()
        {
            _processStartInfo.Verb = "runas";
            _processStartInfo.UseShellExecute = true;
            return this;
        }

        public ProcessBuilder CaptureStdOut(Action<string> outputHandler)
        {
            _processStartInfo.RedirectStandardOutput = true;
            _process.OutputDataReceived += (object sender, DataReceivedEventArgs args) =>
            {
                if (args.Data != null)
                {
                    outputHandler(args.Data);
                }
            };
            return this;
        }

        public ProcessBuilder WithoutCreatingAWindow()
        {
            _processStartInfo.CreateNoWindow = true;
            return this;
        }

        internal ProcessBuilder CaptureJson<T>(Action<T> action)
        {
            _processStartInfo.RedirectStandardOutput = true;
            _process.OutputDataReceived += (object sender, DataReceivedEventArgs args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data))
                {
                    var jsonObject = JsonConvert.DeserializeObject<T>(args.Data);
                    action(jsonObject);
                }
            };
            return this;
        }

        public Process WaitForExit()
        {
            var process = Start();
            process.WaitForExit();
            return process;
        }

        public Process Start()
        {
            try
            {
                _process.Start();
                if (_processStartInfo.RedirectStandardOutput)
                {
                    _process.BeginOutputReadLine();
                }
                return _process;
            }
            catch
            {
                return null;
            }
        }
    }

}
