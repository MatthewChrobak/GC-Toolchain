using Core.ReportGeneration;
using System;
using System.Collections.Generic;
using System.IO;

namespace Core.Logging
{
    public partial class Log
    {
        private readonly bool[] AllowedOutputs = new bool[Enum.GetNames(typeof(OutputLevel)).Length];
        private string _state = default!;
        public string State => _state;
        private Dictionary<string, List<string>> _stateHistory;
        private List<string> AllLines = new List<string>();

        public Log() {
            _stateHistory = new Dictionary<string, List<string>>();
            AllowedOutputs[(int)OutputLevel.Verbose] = false;
            AllowedOutputs[(int)OutputLevel.Warning] = true;
            AllowedOutputs[(int)OutputLevel.Error] = true;
            SetState("Unknown");
        }

        public void Dump(string path) {
            WriteLineVerbose($"Writing log to {path}...");
            File.AppendAllText(path, string.Join('\0', AllLines));
            AllLines = new List<string>();
            _stateHistory = new Dictionary<string, List<string>>();
            SetState("Unknown");
        }

        public void WriteException(Exception e) {
            WriteLineError($"{e.GetType()} :{e.Message}");
        }

        public void SetState(string state) {
            _state = state;
            if (!_stateHistory.ContainsKey(state)) {
                _stateHistory[state] = new List<string>();
            }
        }

        public void WriteLineVerbose(string message) {
            WriteLine(message, OutputLevel.Verbose);
        }

        public void WriteLineVerboseClean(string message) {
            WriteLineClean(message, OutputLevel.Verbose);
        }

        public void WriteLineError(string message) {
            WriteLine(message, OutputLevel.Error);
        }

        public void WriteLineWarning(string message) {
            WriteLine(message, OutputLevel.Warning);
        }

        public void WriteClean(string message, OutputLevel level) {
            if (!AllowedOutputs[(int)level]) {
                return;
            }
            _stateHistory[_state].Add(message);
            AllLines.Add(message);
            Console.Write(message);
        }

        public void WriteLevel(string message, OutputLevel level) {
            WriteClean($"[{level}] - {message}", level);
        }

        public void WriteLine(string message, OutputLevel level) {
            WriteLevel($"{message}\r\n", level);
        }

        public void WriteLineClean(string message, OutputLevel level) {
            WriteClean($"{message}\r\n", level);
        }

        public void EnableLevel(OutputLevel level) {
            AllowedOutputs[(int)level] = true;
        }

        public void DisableLevel(OutputLevel level) {
            AllowedOutputs[(int)level] = false;
        }
    }

    public partial class Log : IReportable
    {
        public IEnumerable<ReportSection> GetReportSections() {
            yield return new LogSection(_stateHistory);
        }
    }
}
