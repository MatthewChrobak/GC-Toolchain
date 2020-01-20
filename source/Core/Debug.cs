using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Core
{
    public class Debug
    {
        [Conditional("DEBUG")]
        public static void Assert(bool condition, string message, [CallerLineNumber] int line = 0, [CallerMemberName] string callingMethod = "unknown", [CallerFilePath] string filepath = "unknown") {
            if (!condition) {
                throw new AssertionFailedException(filepath, line, callingMethod, message);
            }
        }
    }
}
