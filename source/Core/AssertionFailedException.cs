using System;

namespace Core
{
    [Serializable]
    internal class AssertionFailedException : Exception
    {
        public readonly string FileName;
        public readonly string MethodName;
        public readonly int LineNumber;

        public AssertionFailedException(string filepath, int line, string callingMethod, string? message) : base($"Assertion failed in {filepath} on line {line} in the function {callingMethod}. {message}.") {
            Log.WriteLineError(this.Message);
            this.FileName = filepath;
            this.LineNumber = line;
            this.MethodName = callingMethod;
        }
    }
}