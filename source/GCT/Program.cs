using Core;
using Core.Logging;
using Core.ReportGeneration;
using System;

namespace GCT
{
    public class Program
    {
        private const string ReportNameFlag = "r";
        private const string LogNameFlag = "l";
        private const string NoLogFlag = "nolog";
        private const string NoReportFlag = "noreport";
        private const string VerboseFlag = "v";

        public static void Main(string[] args) {

#if DEBUG
            if (args.Length == 0) {
                args = Console.ReadLine()?.Split(' ') ?? Array.Empty<string>();
            }
#endif

            // Nothing to parse, exit early.
            if (args.Length == 0) {
                return;
            }

            Log? log = null;
            Report? report = null;
            var parameters = new ExecutionParameters(args);
            
            if (parameters.Get(NoLogFlag, false) != true) {
                log = new Log();
            }
            if (parameters.Get(NoReportFlag, false) != true) {
                report = new Report();
            }

            if (parameters.Get(VerboseFlag, false) == true) {
                log.EnableLevel(OutputLevel.Verbose);
            }
            
            report?.Add(parameters);


            try {
                var proc = new CompilationProcess(log, report);
                proc.Start(parameters);
            }
            catch (AssertionFailedException e) {
                log?.WriteLineError($"Unable to continue due to exception of type {e.GetType()} being thrown during {log.State}. Exiting.");
                log?.WriteException(e);
            }
            finally {
                if (log is not null) {
                    report?.Add(log);
                }

                report?.Save(parameters.GetCWDRelativePath(ReportNameFlag, "report") + ".html");
                log?.Dump(parameters.GetCWDRelativePath(LogNameFlag, "log") + ".txt");
            }
        }
    }
}
