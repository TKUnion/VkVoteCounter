namespace VKVoteCounter.Engine
{
    public static class StringExtension
    {
        public static string Ellipsis(this string str, int length)
        {
            if (str.Length <= length) return str;
            var pos = str.IndexOf(" ", length);
            if (pos >= 0)
                return str.Substring(0, pos) + "...";
            return str;
        }
    }
}
