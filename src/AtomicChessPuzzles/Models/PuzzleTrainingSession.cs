using ChessDotNet;
using System.Collections.Generic;

namespace AtomicChessPuzzles.Models
{
    public class PuzzleTrainingSession
    {
        public string SessionID { get; private set; }
        public Puzzle Current { get; set; }
        public List<string> SolutionMovesToDo { get; set; }
        public List<string> FENs { get; set; }
        public List<string> PastPuzzleIds { get; set; }

        public PuzzleTrainingSession(string sessionId)
        {
            SessionID = sessionId;
            PastPuzzleIds = new List<string>();
            FENs = new List<string>();
        }

        public void Setup(Puzzle puzzle)
        {
            Current = puzzle;
            SolutionMovesToDo = new List<string>(puzzle.Solutions[0].Split(' '));
            FENs.Clear();
            FENs.Add(puzzle.InitialFen);
        }

        public SubmittedMoveResponse ApplyMove(string origin, string destination, string promotion)
        {
            Piece promotionPiece = null;
            SubmittedMoveResponse response = new SubmittedMoveResponse()
            {
                Success = true,
                Error = null
            };
            if (promotion != null)
            {
                promotionPiece = Utilities.GetPromotionPieceFromName(promotion, Current.Game.WhoseTurn);
                if (promotionPiece == null)
                {
                    response.Success = false;
                    response.Error = "Invalid promotion piece.";
                    response.Correct = SubmittedMoveResponse.INVALID_MOVE;
                    return response;
                }
            }

            MoveType type = Current.Game.ApplyMove(new Move(origin, destination, Current.Game.WhoseTurn, promotionPiece), false);
            if (type == MoveType.Invalid)
            {
                response.Success = false;
                response.Error = "Invalid move.";
                response.Correct = SubmittedMoveResponse.INVALID_MOVE;
            }

            response.Check = Current.Game.IsInCheck(Current.Game.WhoseTurn) ? Current.Game.WhoseTurn.ToString().ToLowerInvariant() : null;
            string fen = Current.Game.GetFen();
            FENs.Add(fen);

            if (Current.Game.IsCheckmated(Current.Game.WhoseTurn) || Current.Game.KingIsGone(Current.Game.WhoseTurn))
            {
                response.Correct = 1;
                response.FEN = fen;
                response.ExplanationSafe = Current.ExplanationSafe;
                PastPuzzleIds.Add(Current.ID);
                response.FENs = FENs;
                return response;
            }

            if (string.Compare(SolutionMovesToDo[0], origin + "-" + destination + (promotion != null ? "=" + char.ToUpperInvariant(promotionPiece.GetFenCharacter()) : ""), true) != 0)
            {
                response.Correct = -1;
                response.Solution = Current.Solutions[0];
                response.ExplanationSafe = Current.ExplanationSafe;
                PastPuzzleIds.Add(Current.ID);
                foreach (string move in SolutionMovesToDo)
                {
                    string[] p = move.Split('-', '=');
                    Current.Game.ApplyMove(new Move(p[0], p[1], Current.Game.WhoseTurn, p.Length == 2 ? null : Utilities.GetPromotionPieceFromChar(p[2][0], Current.Game.WhoseTurn)), true);
                    FENs.Add(Current.Game.GetFen());
                }
                response.FENs = FENs;
                return response;
            }

            SolutionMovesToDo.RemoveAt(0);
            if (SolutionMovesToDo.Count == 0)
            {
                response.Correct = 1;
                response.Solution = Current.Solutions[0];
                response.FEN = fen;
                response.ExplanationSafe = Current.ExplanationSafe;
                PastPuzzleIds.Add(Current.ID);
                response.FENs = FENs;
                return response;
            }

            response.FEN = fen;

            string moveToPlay = SolutionMovesToDo[0];
            string[] parts = moveToPlay.Split('-', '=');
            Current.Game.ApplyMove(new Move(parts[0], parts[1], Current.Game.WhoseTurn, parts.Length == 2 ? null : Utilities.GetPromotionPieceFromChar(parts[2][0], Current.Game.WhoseTurn)), true);
            response.Play = moveToPlay;
            response.FenAfterPlay = Current.Game.GetFen();
            FENs.Add(response.FenAfterPlay);
            response.CheckAfterAutoMove = Current.Game.IsInCheck(Current.Game.WhoseTurn) ? Current.Game.WhoseTurn.ToString().ToLowerInvariant() : null;
            response.Moves = Current.Game.GetValidMoves(Current.Game.WhoseTurn);
            response.Correct = 0;
            SolutionMovesToDo.RemoveAt(0);
            if (SolutionMovesToDo.Count == 0)
            {
                response.Correct = 1;
                response.ExplanationSafe = Current.ExplanationSafe;
                PastPuzzleIds.Add(Current.ID);
                response.FENs = FENs;
            }
            return response;
        }
    }
}
