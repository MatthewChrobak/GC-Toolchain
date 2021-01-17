namespace GCT
{
    public static class Extensions
    {
        public static bool StartsAndEndsWith(this string value, char @char) {
            return value.StartsWith(@char) && value.EndsWith(@char);
        }
    }
}
