using ChessDotNet;
using ChessDotNet.Pieces;

namespace AtomicChessPuzzles
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

        public static Piece GetPromotionPieceFromName(string piece, Player owner)
        {
            switch (piece)
            {
                case "queen":
                    return new Queen(owner);
                case "knight":
                    return new Knight(owner);
                case "rook":
                    return new Rook(owner);
                case "bishop":
                    return new Bishop(owner);
                default:
                    return null;
            }
        }
    }
}