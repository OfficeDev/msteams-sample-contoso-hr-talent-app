namespace TeamsTalentMgmtAppV3.Extensions
{
    public static class StringExtensions
    {
        public static bool HasValue(this string s) => !string.IsNullOrEmpty(s);
        
        public static string NormalizeUtterance(this string utterance)
            => utterance?
                   .Trim()
               ?? string.Empty;
    }
}