using System;

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

        public static string NormalizeVariantNameCapitalization(string variant)
        {
            string variantUpper = variant.ToUpperInvariant();
            switch (variantUpper)
            {
                case "ANTICHESS":
                    return "Antichess";
                case "ATOMIC":
                    return "Atomic";
                case "HORDE":
                    return "Horde";
                case "KINGOFTHEHILL":
                    return "KingOfTheHill";
                case "RACINGKINGS":
                    return "RacingKings";
                case "THREECHECK":
                    return "ThreeCheck";
                case "MIXED":
                    return "Mixed";
                default:
                    return variant;
            }
        }
    }
}