using ChessDotNet;
using System;
using System.Collections.Generic;

namespace AtomicChessPuzzles.Models
{
    public class PuzzleDuringTraining
    {
        public Puzzle Puzzle
        {
            get;
            set;
        }

        public string TrainingSessionId
        {
            get;
            set;
        }

        public List<string> SolutionMovesToDo
        {
            get;
            set;
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
                promotionPiece = Utilities.GetPromotionPieceFromName(promotion, Puzzle.Game.WhoseTurn);
                if (promotionPiece == null)
                {
                    response.Success = false;
                    response.Error = "Invalid promotion piece.";
                    response.Correct = SubmittedMoveResponse.INVALID_MOVE;
                    return response;
                }
            }

            MoveType type = Puzzle.Game.ApplyMove(new Move(origin, destination, Puzzle.Game.WhoseTurn, promotionPiece), false);
            if (type == MoveType.Invalid)
            {
                response.Success = false;
                response.Error = "Invalid move.";
                response.Correct = SubmittedMoveResponse.INVALID_MOVE;
            }

            response.Check = Puzzle.Game.IsInCheck(Puzzle.Game.WhoseTurn) ? Puzzle.Game.WhoseTurn.ToString().ToLowerInvariant() : null;

            if (Puzzle.Game.IsCheckmated(Puzzle.Game.WhoseTurn) || Puzzle.Game.KingIsGone(Puzzle.Game.WhoseTurn))
            {
                response.Correct = 1;
                response.FEN = Puzzle.Game.GetFen();
                response.ExplanationSafe = Puzzle.ExplanationSafe;
                return response;
            }

            if (string.Compare(SolutionMovesToDo[0], origin + "-" + destination + (promotion != null ? "=" + char.ToUpperInvariant(promotionPiece.GetFenCharacter()) : ""), true) != 0)
            {
                response.Correct = -1;
                response.Solution = Puzzle.Solutions[0];
                response.ExplanationSafe = Puzzle.ExplanationSafe;
                return response;
            }

            SolutionMovesToDo.RemoveAt(0);
            if (SolutionMovesToDo.Count == 0)
            {
                response.Correct = 1;
                response.Solution = Puzzle.Solutions[0];
                response.FEN = Puzzle.Game.GetFen();
                response.ExplanationSafe = Puzzle.ExplanationSafe;
                return response;
            }

            response.FEN = Puzzle.Game.GetFen();

            string moveToPlay = SolutionMovesToDo[0];
            string[] parts = moveToPlay.Split('-', '=');
            Puzzle.Game.ApplyMove(new Move(parts[0], parts[1], Puzzle.Game.WhoseTurn, parts.Length == 2 ? null : Utilities.GetPromotionPieceFromChar(parts[2][0], Puzzle.Game.WhoseTurn)), true);
            response.Play = moveToPlay;
            response.FenAfterPlay = Puzzle.Game.GetFen();
            response.CheckAfterAutoMove = Puzzle.Game.IsInCheck(Puzzle.Game.WhoseTurn) ? Puzzle.Game.WhoseTurn.ToString().ToLowerInvariant() : null;
            response.Moves = Puzzle.Game.GetValidMoves(Puzzle.Game.WhoseTurn);
            response.Correct = 0;
            SolutionMovesToDo.RemoveAt(0);
            if (SolutionMovesToDo.Count == 0)
            {
                response.Correct = 1;
                response.ExplanationSafe = Puzzle.ExplanationSafe;
            }
            return response;
        }
    }
}
