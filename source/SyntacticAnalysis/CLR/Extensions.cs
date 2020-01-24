using System.Collections.Generic;

namespace SyntacticAnalysis.CLR
{
    public static class Extensions
    {
        public static bool HasSameElementsAs<T>(this HashSet<T> hs1, HashSet<T> hs2) {
            if (hs1.Count != hs2.Count) {
                return false;
            }
            foreach (var element in hs1) {
                if (!hs2.Contains(element)) {
                    return false;
                }
            }
            return true;
        }
    }
}
