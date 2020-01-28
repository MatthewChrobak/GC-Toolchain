using System.Collections.Generic;

namespace SyntacticAnalysis.CLR
{
    public static class Extensions
    {
        public static bool HasSameElementsAs<T>(this HashSet<T> s1, HashSet<T> s2) {
            foreach (var element in s1) {
                if (!s2.Contains(element)) {
                    return false;
                }
            }
            return true;
        }
    }
}
