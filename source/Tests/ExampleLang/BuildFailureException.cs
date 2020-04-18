using System;
using System.Runtime.Serialization;

namespace Tests.ExampleLang
{
    [Serializable]
    internal class BuildFailureException : Exception
    {
        public BuildFailureException() {
        }

        public BuildFailureException(string? message) : base(message) {
        }

        public BuildFailureException(string? message, Exception? innerException) : base(message, innerException) {
        }

        protected BuildFailureException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}