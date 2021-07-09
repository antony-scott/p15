using Microsoft.VisualStudio.TestTools.UnitTesting;
using p15.Core.Models;
using p15.Core.Parsers;
using Shouldly;
using System.Collections.Generic;
using System.Linq;

namespace p15.Core.Tests.Unit
{
    [TestClass]
    public class Parse
    {
        private List<LogEntryModel> TestParser(IEnumerable<string> lines)
        {
            var logEntries = new List<LogEntryModel>();
            var parser = new AdbLogParser();
            foreach (var line in lines)
            {
                var logEntry = parser.Parse(line);
                if (logEntry != null)
                {
                    logEntries.Add(logEntry);
                }
            }
            return logEntries;
        }

        [TestMethod]
        public void NoData()
        {
            var results = TestParser(new string[] { null });
            results.ShouldBeEmpty();
        }

        [TestMethod]
        public void EmptyData()
        {
            var results = TestParser(new[] { "" });
            results.ShouldBeEmpty();
        }

        [TestMethod]
        public void PreambleData()
        {
            var results = TestParser(new[] { "--------- beginning of crash" });
            results.ShouldBeEmpty();
        }

        [TestMethod]
        public void SingleLineLogEntry()
        {
            var results = TestParser(new[]
            {
                "[ 03-14 21:59:16.310 16119:14751 I/appname ]",
                "EVENTSYNCSERVICE: ... found 1 events to send",
                ""
            });
            results.Count().ShouldBe(1);
        }

        [TestMethod]
        public void MultiLineLogEntry()
        {
            var results = TestParser(new[]
            {
                "[ 03-14 21:59:45.586 16119:14751 E/appname ]",
                "Connection timed out",
                "  at System.Net.Http.ConnectHelper.ConnectAsync (System.String host, System.Int32 port, System.Threading.CancellationToken cancellationToken) [0x001ac] in <f7c7c46be5f445eda65ec71dfd718cb6>:0 ",
                "  at System.Net.Http.HttpConnectionPool.CreateConnectionAsync (System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken) [0x00134] in <f7c7c46be5f445eda65ec71dfd718cb6>:0 ",
                "  at System.Net.Http.HttpConnectionPool.WaitForCreatedConnectionAsync (System.Threading.Tasks.ValueTask`1[TResult] creationTask) [0x000a2] in <f7c7c46be5f445eda65ec71dfd718cb6>:0 ",
                "  at System.Net.Http.HttpConnectionPool.SendWithRetryAsync (System.Net.Http.HttpRequestMessage request, System.Boolean doRequestAuth, System.Threading.CancellationToken cancellationToken) [0x00089] in <f7c7c46be5f445eda65ec71dfd718cb6>:0 ",
                "  at System.Net.Http.RedirectHandler.SendAsync (System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken) [0x000ba] in <f7c7c46be5f445eda65ec71dfd718cb6>:0 ",
                "  at System.Net.Http.HttpClient.FinishSendAsyncBuffered (System.Threading.Tasks.Task`1[TResult] sendTask, System.Net.Http.HttpRequestMessage request, System.Threading.CancellationTokenSource cts, System.Boolean disposeCts) [0x0017e] in <f7c7c46be5f445eda65ec71dfd718cb6>:0 ",
                "  at CompanyName.AppName.Droid.Services.DroidHttpService.SendAsync (System.Net.Http.HttpRequestMessage request, CompanyName.AppName.Core.Enums.HttpServerType serverType) [0x000cc] in <b21f42a0d9684af395265c55ad9cdaf0>:0 ",
                ""
            });
            results.Count().ShouldBe(1);
        }

        [TestMethod]
        public void MixtureOfLogEntries()
        {
            var results = TestParser(new[]
            {
                "--------- beginning of crash",
                "--------- beginning of system",
                "--------- beginning of main",
                "[ 03-14 21:59:16.310 16119:14751 I/appname ]",
                "EVENTSYNCSERVICE: ... found 1 events to send",
                "",
                "[ 03-14 21:59:45.586 16119:14751 E/appname ]",
                "Connection timed out",
                "  at System.Net.Http.ConnectHelper.ConnectAsync (System.String host, System.Int32 port, System.Threading.CancellationToken cancellationToken) [0x001ac] in <f7c7c46be5f445eda65ec71dfd718cb6>:0 ",
                "  at System.Net.Http.HttpConnectionPool.CreateConnectionAsync (System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken) [0x00134] in <f7c7c46be5f445eda65ec71dfd718cb6>:0 ",
                "  at System.Net.Http.HttpConnectionPool.WaitForCreatedConnectionAsync (System.Threading.Tasks.ValueTask`1[TResult] creationTask) [0x000a2] in <f7c7c46be5f445eda65ec71dfd718cb6>:0 ",
                "  at System.Net.Http.HttpConnectionPool.SendWithRetryAsync (System.Net.Http.HttpRequestMessage request, System.Boolean doRequestAuth, System.Threading.CancellationToken cancellationToken) [0x00089] in <f7c7c46be5f445eda65ec71dfd718cb6>:0 ",
                "  at System.Net.Http.RedirectHandler.SendAsync (System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken) [0x000ba] in <f7c7c46be5f445eda65ec71dfd718cb6>:0 ",
                "  at System.Net.Http.HttpClient.FinishSendAsyncBuffered (System.Threading.Tasks.Task`1[TResult] sendTask, System.Net.Http.HttpRequestMessage request, System.Threading.CancellationTokenSource cts, System.Boolean disposeCts) [0x0017e] in <f7c7c46be5f445eda65ec71dfd718cb6>:0 ",
                "  at CompanyName.AppName.Droid.Services.DroidHttpService.SendAsync (System.Net.Http.HttpRequestMessage request, CompanyName.AppName.Core.Enums.HttpServerType serverType) [0x000cc] in <b21f42a0d9684af395265c55ad9cdaf0>:0 ",
                "",
                "[ 03-14 21:59:45.600 16119:14750 I/appname ]",
                "Failed to connect to http://172.20.1.30:50009/v1/ (discover) (OperationID: 515a918b-7171-43c0-bd7c-fa8e2314b5b3)",
                "",
                "[ 03-14 21:59:45.623 16119:14755 I/appname ]",
                "Scheduling next SYNC for 3/14/2020 10:07:45 PM",
                "",
                "[ 03-14 21:59:47.426 16119:14781 E/appname ]",
                "Connection timed out",
                "  at System.Net.Http.ConnectHelper.ConnectAsync (System.String host, System.Int32 port, System.Threading.CancellationToken cancellationToken) [0x001ac] in <f7c7c46be5f445eda65ec71dfd718cb6>:0 ",
                "  at System.Net.Http.HttpConnectionPool.CreateConnectionAsync (System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken) [0x00134] in <f7c7c46be5f445eda65ec71dfd718cb6>:0 ",
                "  at System.Net.Http.HttpConnectionPool.WaitForCreatedConnectionAsync (System.Threading.Tasks.ValueTask`1[TResult] creationTask) [0x000a2] in <f7c7c46be5f445eda65ec71dfd718cb6>:0 ",
                "  at System.Net.Http.HttpConnectionPool.SendWithRetryAsync (System.Net.Http.HttpRequestMessage request, System.Boolean doRequestAuth, System.Threading.CancellationToken cancellationToken) [0x00089] in <f7c7c46be5f445eda65ec71dfd718cb6>:0 ",
                "  at System.Net.Http.RedirectHandler.SendAsync (System.Net.Http.HttpRequestMessage request, System.Threading.CancellationToken cancellationToken) [0x000ba] in <f7c7c46be5f445eda65ec71dfd718cb6>:0 ",
                "  at System.Net.Http.HttpClient.FinishSendAsyncBuffered (System.Threading.Tasks.Task`1[TResult] sendTask, System.Net.Http.HttpRequestMessage request, System.Threading.CancellationTokenSource cts, System.Boolean disposeCts) [0x0017e] in <f7c7c46be5f445eda65ec71dfd718cb6>:0 ",
                "  at CompanyName.AppName.Droid.Services.DroidHttpService.SendAsync (System.Net.Http.HttpRequestMessage request, CompanyName.AppName.Core.Enums.HttpServerType serverType) [0x000cc] in <b21f42a0d9684af395265c55ad9cdaf0>:0 ",
                ""
            });
            results.Count().ShouldBe(5);
        }

        // TODO: add some tests to check log messages half way through being written (ie - no trailing empty line)
        // TODO: make sure the file length is correct in these new tests and all existing ones
    }
}
