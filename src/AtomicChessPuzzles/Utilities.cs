using ChessDotNet;
using ChessDotNet.Pieces;

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

        public static Piece GetPromotionPieceFromChar(char piece, Player owner)
        {
            switch (piece)
            {
                case 'Q':
                    return new Queen(owner);
                case 'N':
                    return new Knight(owner);
                case 'R':
                    return new Rook(owner);
                case 'B':
                    return new Bishop(owner);
                default:
                    return null;
            }
        }
    }
}