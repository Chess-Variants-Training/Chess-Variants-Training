using ChessDotNet;
using ChessVariantsTraining.Services;
using System;
using System.Collections.Generic;

namespace ChessVariantsTraining.Models
{
    public class PuzzleTrainingSession
    {
        public string SessionID { get; private set; }
        public Puzzle Current { get; set; }
        public List<string> SolutionMovesToDo { get; set; }
        public List<string> FENs { get; set; }
        public List<string> Checks { get; set; }
        public List<string> Moves { get; set; }
        public List<int> PastPuzzleIds { get; set; }
        public DateTime? CurrentPuzzleStartedUtc { get; set; }
        public DateTime? CurrentPuzzleEndedUtc { get; set; }

        IGameConstructor gameConstructor;

        public PuzzleTrainingSession(string sessionId, IGameConstructor _gameConstructor)
        {
            SessionID = sessionId;
            PastPuzzleIds = new List<int>();
            FENs = new List<string>();
            Checks = new List<string>();
            Moves = new List<string>();

            gameConstructor = _gameConstructor;
        }

        public void Setup(Puzzle puzzle)
        {
            Current = puzzle;
            CurrentPuzzleStartedUtc = DateTime.UtcNow;
            CurrentPuzzleEndedUtc = null;
            SolutionMovesToDo = new List<string>(puzzle.Solutions[0].Split(' '));
            FENs.Clear();
            FENs.Add(puzzle.InitialFen);
            Checks.Clear();
            Checks.Add(Current.Game.IsInCheck(Current.Game.WhoseTurn) ? Current.Game.WhoseTurn.ToString().ToLowerInvariant() : null);
            Moves.Clear();
            Moves.Add(null);
        }

        public SubmittedMoveResponse ApplyMove(string origin, string destination, string promotion)
        {
            SubmittedMoveResponse response = new SubmittedMoveResponse()
            {
                Success = true,
                Error = null
            };
            if (promotion != null && promotion.Length != 1)
            {
                response.Success = false;
                response.Error = "Invalid promotion.";
                response.Correct = SubmittedMoveResponse.INVALID_MOVE;
                return response;
            }

            MoveType type = Current.Game.ApplyMove(new Move(origin, destination, Current.Game.WhoseTurn, promotion?[0]), false);
            if (type == MoveType.Invalid)
            {
                response.Success = false;
                response.Error = "Invalid move.";
                response.Correct = SubmittedMoveResponse.INVALID_MOVE;
            }

            response.Check = Current.Game.IsInCheck(Current.Game.WhoseTurn) ? Current.Game.WhoseTurn.ToString().ToLowerInvariant() : null;
            Checks.Add(response.Check);
            string fen = Current.Game.GetFen();
            response.FEN = fen;
            FENs.Add(fen);

            string promotionUpper = promotion.ToUpperInvariant();
            Moves.Add(string.Format("{0}-{1}={2}", origin, destination, promotion == null ? "" : "=" + promotionUpper));

            if (Current.Game.IsWinner(ChessUtilities.GetOpponentOf(Current.Game.WhoseTurn)))
            {
                PuzzleFinished(response, true);
                return response;
            }

            if (string.Compare(SolutionMovesToDo[0], origin + "-" + destination + (promotion != null ? "=" + promotionUpper : ""), true) != 0)
            {
                PuzzleFinished(response, false);
                return response;
            }

            SolutionMovesToDo.RemoveAt(0);
            if (SolutionMovesToDo.Count == 0)
            {
                PuzzleFinished(response, true);
                return response;
            }

            response.FEN = fen;

            string moveToPlay = SolutionMovesToDo[0];
            string[] parts = moveToPlay.Split('-', '=');
            Current.Game.ApplyMove(new Move(parts[0], parts[1], Current.Game.WhoseTurn, parts.Length == 2 ? null : new char?(parts[2][0])), true);
            response.Play = moveToPlay;
            Moves.Add(moveToPlay);
            response.FenAfterPlay = Current.Game.GetFen();
            FENs.Add(response.FenAfterPlay);
            response.CheckAfterAutoMove = Current.Game.IsInCheck(Current.Game.WhoseTurn) ? Current.Game.WhoseTurn.ToString().ToLowerInvariant() : null;
            Checks.Add(response.CheckAfterAutoMove);
            response.Moves = Current.Game.GetValidMoves(Current.Game.WhoseTurn);
            response.Correct = 0;
            SolutionMovesToDo.RemoveAt(0);
            if (SolutionMovesToDo.Count == 0)
            {
                PuzzleFinished(response, true);
            }
            return response;
        }

        void PuzzleFinished(SubmittedMoveResponse response, bool correct)
        {
            CurrentPuzzleEndedUtc = DateTime.UtcNow;

            response.Correct = correct ? 1 : -1;
            response.ExplanationSafe = Current.ExplanationSafe;

            PastPuzzleIds.Add(Current.ID);

            if (!correct)
            {
                Moves.RemoveAt(Moves.Count - 1);
                FENs.RemoveAt(FENs.Count - 1);
                Checks.RemoveAt(Checks.Count - 1);

                response.FEN = FENs[FENs.Count - 1];

                ChessGame correctGame = gameConstructor.Construct(Current.Variant, response.FEN);
                foreach (string move in SolutionMovesToDo)
                {
                    string[] p = move.Split('-', '=');
                    correctGame.ApplyMove(new Move(p[0], p[1], correctGame.WhoseTurn, p.Length == 2 ? null : new char?(p[2][0])), true);
                    FENs.Add(correctGame.GetFen());
                    Checks.Add(correctGame.IsInCheck(correctGame.WhoseTurn) ? correctGame.WhoseTurn.ToString().ToLowerInvariant() : null);
                    Moves.Add(move);
                }
            }
            response.ReplayFENs = FENs;
            response.ReplayChecks = Checks;
            response.ReplayMoves = Moves;
        }
    }
}
