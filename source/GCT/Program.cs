using Core;
using Core.Logging;
using Core.ReportGeneration;

namespace GCT
{
    internal static class Program2
    {
        public static void Main2(string[] args) {
            // Nothing to parse, exit early.
            if (args.Length == 0) {
                return;
            }

            Log? log = null;
            var report = new Report();
            string reportPath = "report.html";
            string logDumpPath = "dmp.log";

            try {
                var parameters = new ExecutionParameters(args);
                report.Add(parameters);
            }
            catch (AssertionFailedException e) {
                log?.WriteLineError($"Unable to continue due to exception of type {e.GetType()} being thrown during {log.State}. Exiting.");
                log?.WriteException(e);
            }
            finally {
                if (log is not null) {
                    report.Add(log);
                }
                report.Save(reportPath);
                log?.Dump(logDumpPath);
            }
        }
    }
}
