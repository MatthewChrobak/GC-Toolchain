using Core.ReportGeneration;
using System;
using System.Collections.Generic;
using System.IO;

namespace Core
{
    public static class Log
    {
        private static readonly bool[] AllowedOutputs = new bool[Enum.GetNames(typeof(OutputLevel)).Length];
        private static string _state;
        public static string State => _state;
        private static Dictionary<string, List<string>> _stateHistory;
        private static List<string> AllLines = new List<string>();

        static Log() {
            _stateHistory = new Dictionary<string, List<string>>();
            AllowedOutputs[(int)OutputLevel.Verbose] = false;
            AllowedOutputs[(int)OutputLevel.Warning] = true;
            AllowedOutputs[(int)OutputLevel.Error] = true;
            SetState("Unknown");
        }

        public static void Dump(string path) {
            File.AppendAllLines(path, AllLines);
            AllLines = new List<string>();
            _stateHistory = new Dictionary<string, List<string>>();
            SetState("Unknown");
        }

        public static void SetState(string state) {
            _state = state;
            if (!_stateHistory.ContainsKey(state)) {
                _stateHistory[state] = new List<string>();
            }
        }

        public static void WriteLineVerbose(string message) {
            WriteLine(message, OutputLevel.Verbose);
        }

        public static void WriteLineVerboseClean(string message) {
            WriteLineClean(message, OutputLevel.Verbose);
        }

        public static void WriteLineError(string message) {
            WriteLine(message, OutputLevel.Error);
        }

        public static void WriteLineWarning(string message) {
            WriteLine(message, OutputLevel.Warning);
        }


        public static void WriteClean(string message, OutputLevel level) {
            if (!AllowedOutputs[(int)level]) {
                return;
            }
            _stateHistory[_state].Add(message);
            AllLines.Add(message);
            Console.Write(message);
        }

        public static void WriteLevel(string message, OutputLevel level) {
            WriteClean($"[{level}] - {message}", level);
        }

        public static void WriteLine(string message, OutputLevel level) {
            WriteLevel($"{message}\r\n", level);
        }

        public static void WriteLineClean(string message, OutputLevel level) {
            WriteClean($"{message}\r\n", level);
        }

        public static void EnableLevel(OutputLevel level) {
            AllowedOutputs[(int)level] = true;
        }

        public static void DisableLevel(OutputLevel level) {
            AllowedOutputs[(int)level] = false;
        }

        public static ReportSection GetReportSections() {
            return new LogSection(_stateHistory);
        }
    }
}
