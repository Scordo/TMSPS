namespace System
{
    public static class ExtensionMethods
    {
        public static bool IsNullOrTimmedEmpty(this string text)
        {
            return text == null || text.Trim().Length == 0;
        }
    }
}
