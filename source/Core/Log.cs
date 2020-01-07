using System;

namespace Core
{
    public static class Log
    {
        private static bool[] AllowedOutputs = new bool[Enum.GetNames(typeof(OutputLevel)).Length];

        static Log() {
            AllowedOutputs[(int)OutputLevel.Verbose] = false;
            AllowedOutputs[(int)OutputLevel.Warning] = true;
            AllowedOutputs[(int)OutputLevel.Error] = true;
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
            Console.WriteLine(message);
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
    }
}
