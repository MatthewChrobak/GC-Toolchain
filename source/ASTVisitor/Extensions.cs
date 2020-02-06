using System.Text;

namespace ASTVisitor
{
    public static class Extensions
    {
        public static string Multiply(this string value, int count) {
            var sb = new StringBuilder();
            for (int i = 0; i < count; i++) {
                sb.Append(value);
            }
            return sb.ToString();
        }
    }
}
