namespace ChessVariantsTraining
{
    public static class Utilities
    {
        public static string SanitizeHtml(string unsafeHtml)
        {
            return unsafeHtml.Replace("&", "&amp;")
                             .Replace("<", "&lt;")
                             .Replace(">", "&gt;")
                             .Replace("\"", "&quot;");
        }
    }
}