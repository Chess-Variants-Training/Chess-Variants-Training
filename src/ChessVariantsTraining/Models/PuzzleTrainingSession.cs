using ChessDotNet;
using ChessDotNet.Variants.Crazyhouse;
using ChessVariantsTraining.Extensions;
using ChessVariantsTraining.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessVariantsTraining.Models
{
    public class PuzzleTrainingSession
    {
        public string SessionID { get; private set; }
        public Puzzle Current { get; set; }
        public List<string> FENs { get; set; }
        public List<string> Checks { get; set; }
        public List<string> Moves { get; set; }
        public List<Dictionary<string, int>> Pockets { get; set; }
        public List<int> PastPuzzleIds { get; set; }
        public DateTime? CurrentPuzzleStartedUtc { get; set; }
        public DateTime? CurrentPuzzleEndedUtc { get; set; }

        IEnumerable<IEnumerable<string>> PossibleVariations = null;

        IGameConstructor gameConstructor;

        public PuzzleTrainingSession(string sessionId, IGameConstructor _gameConstructor)
        {
            SessionID = sessionId;
            PastPuzzleIds = new List<int>();
            FENs = new List<string>();
            Checks = new List<string>();
            Moves = new List<string>();
            Pockets = new List<Dictionary<string, int>>();

            gameConstructor = _gameConstructor;
        }

        public void Setup(Puzzle puzzle)
        {
            Current = puzzle;
            CurrentPuzzleStartedUtc = DateTime.UtcNow;
            CurrentPuzzleEndedUtc = null;
            PossibleVariations = puzzle.Solutions.Select(x => x.Split(' '));
            FENs.Clear();
            FENs.Add(puzzle.InitialFen);
            Checks.Clear();
            Checks.Add(Current.Game.IsInCheck(Current.Game.WhoseTurn) ? Current.Game.WhoseTurn.ToString().ToLowerInvariant() : null);
            Moves.Clear();
            Moves.Add(null);
            Pockets.Clear();
            Pockets.Add(Current.Game.GenerateJsonPocket());
        }

        int CompareMoves(string move1, string move2)
        {
            if (move1.Contains("@") && move1.Length == 3)
            {
                move1 = "P" + move1;
            }
            if (move2.Contains("@") & move2.Length == 3)
            {
                move2 = "P" + move2;
            }
            return string.Compare(move1, move2, true);
        }

        SubmittedMoveResponse ApplyMoveAndDropCommon(SubmittedMoveResponse response, string moveStr)
        {
            response.Check = Current.Game.IsInCheck(Current.Game.WhoseTurn) ? Current.Game.WhoseTurn.ToString().ToLowerInvariant() : null;
            Checks.Add(response.Check);
            string fen = Current.Game.GetFen();
            response.FEN = fen;
            FENs.Add(fen);
            Dictionary<string, int> pocket = Current.Game.GenerateJsonPocket();
            response.Pocket = pocket;
            Pockets.Add(pocket);

            if (Current.Game.IsWinner(ChessUtilities.GetOpponentOf(Current.Game.WhoseTurn)))
            {
                PuzzleFinished(response, true);
                return response;
            }

            if (!PossibleVariations.Any(x => CompareMoves(x.First(), moveStr) == 0))
            {
                PuzzleFinished(response, false);
                return response;
            }

            PossibleVariations = PossibleVariations.Where(x => CompareMoves(x.First(), moveStr) == 0).Select(x => x.Skip(1));

            if (PossibleVariations.Any(x => x.Count() == 0))
            {
                PuzzleFinished(response, true);
                return response;
            }

            string moveToPlay = PossibleVariations.First().First();

            if (!moveToPlay.Contains("@"))
            {
                string[] parts = moveToPlay.Split('-', '=');
                Current.Game.ApplyMove(new Move(parts[0], parts[1], Current.Game.WhoseTurn, parts.Length == 2 ? null : new char?(parts[2][0])), true);
            }
            else
            {
                string[] parts = moveToPlay.Split('@');

                CrazyhouseChessGame zhCurrent = Current.Game as CrazyhouseChessGame;
                Piece toDrop = zhCurrent.MapPgnCharToPiece(parts[0] == "" ? 'P' : parts[0][0], zhCurrent.WhoseTurn);
                Drop drop = new Drop(toDrop, new Position(parts[1]), zhCurrent.WhoseTurn);
                zhCurrent.ApplyDrop(drop, true);
            }

            response.Play = moveToPlay;
            Moves.Add(moveToPlay);
            response.FenAfterPlay = Current.Game.GetFen();
            FENs.Add(response.FenAfterPlay);
            response.CheckAfterAutoMove = Current.Game.IsInCheck(Current.Game.WhoseTurn) ? Current.Game.WhoseTurn.ToString().ToLowerInvariant() : null;
            Checks.Add(response.CheckAfterAutoMove);
            response.Moves = Current.Game.GetValidMoves(Current.Game.WhoseTurn);
            response.Correct = 0;
            response.PocketAfterAutoMove = Current.Game.GenerateJsonPocket();
            Pockets.Add(response.PocketAfterAutoMove);
            PossibleVariations = PossibleVariations.Select(x => x.Skip(1));
            if (PossibleVariations.Any(x => !x.Any()))
            {
                PuzzleFinished(response, true);
            }
            return response;
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

            string promotionUpper = promotion?.ToUpperInvariant();
            Moves.Add(string.Format("{0}-{1}{2}", origin, destination, promotion == null ? "" : "=" + promotionUpper));

            string moveStr = origin + "-" + destination + (promotion != null ? "=" + promotionUpper : "");

            return ApplyMoveAndDropCommon(response, moveStr);
        }

        public SubmittedMoveResponse ApplyDrop(string role, string pos)
        {
            SubmittedMoveResponse response = new SubmittedMoveResponse()
            {
                Success = true,
                Error = null
            };

            if (!(Current.Game is CrazyhouseChessGame))
            {
                response.Success = false;
                response.Error = "Not a crazyhouse puzzle.";
                response.Correct = SubmittedMoveResponse.INVALID_MOVE;
                return response;
            }

            CrazyhouseChessGame zhCurrentGame = Current.Game as CrazyhouseChessGame;
            Piece p = Utilities.GetByRole(role, zhCurrentGame.WhoseTurn);
            if (p == null)
            {
                response.Success = false;
                response.Error = "Invalid drop piece.";
                response.Correct = SubmittedMoveResponse.INVALID_MOVE;
                return response;
            }
            Drop drop = new Drop(p, new Position(pos), zhCurrentGame.WhoseTurn);

            if (!zhCurrentGame.ApplyDrop(drop, false))
            {
                response.Correct = SubmittedMoveResponse.INVALID_MOVE;
                return response;
            }

            char pieceChar = char.ToUpper(p.GetFenCharacter());
            string moveStr = (pieceChar == 'P' ? "" : pieceChar.ToString()) + "@" + pos;
            Moves.Add(moveStr);

            return ApplyMoveAndDropCommon(response, moveStr);
        }

        void PuzzleFinished(SubmittedMoveResponse response, bool correct)
        {
            CurrentPuzzleEndedUtc = DateTime.UtcNow;

            response.Correct = correct ? 1 : -1;
            response.ExplanationSafe = Current.ExplanationSafe;

            if (!PastPuzzleIds.Contains(Current.ID))
            {
                PastPuzzleIds.Add(Current.ID);
            }

            List<string> replayFens = new List<string>(FENs);
            List<string> replayChecks = new List<string>(Checks);
            List<string> replayMoves = new List<string>(Moves);
            List<Dictionary<string, int>> replayPockets = new List<Dictionary<string, int>>(Pockets);
            if (!correct)
            {
                Moves.RemoveAt(Moves.Count - 1);
                FENs.RemoveAt(FENs.Count - 1);
                Checks.RemoveAt(Checks.Count - 1);
                Pockets.RemoveAt(Pockets.Count - 1);

                replayFens.RemoveAt(replayFens.Count - 1);
                replayMoves.RemoveAt(replayMoves.Count - 1);
                replayChecks.RemoveAt(replayChecks.Count - 1);
                replayPockets.RemoveAt(replayPockets.Count - 1);

                response.FEN = FENs[FENs.Count - 1];
                response.Pocket = Pockets[Pockets.Count - 1];

                ChessGame correctGame = gameConstructor.Construct(Current.Variant, Current.InitialFen);
                int i = 0;
                var full = replayMoves.Concat(PossibleVariations.First());
                foreach (string move in full)
                {
                    if (move == null) { i++; continue; }
                    if (!move.Contains("@"))
                    {
                        string[] p = move.Split('-', '=');
                        correctGame.ApplyMove(new Move(p[0], p[1], correctGame.WhoseTurn, p.Length == 2 ? null : new char?(p[2][0])), true);
                    }
                    else
                    {
                        string[] p = move.Split('@');
                        if (string.IsNullOrEmpty(p[0])) p[0] = "P";
                        Drop drop = new Drop(correctGame.MapPgnCharToPiece(p[0][0], correctGame.WhoseTurn), new Position(p[1]), correctGame.WhoseTurn);
                        (correctGame as CrazyhouseChessGame).ApplyDrop(drop, true);
                    }
                    if (i >= Moves.Count)
                    {
                        replayFens.Add(correctGame.GetFen());
                        replayChecks.Add(correctGame.IsInCheck(correctGame.WhoseTurn) ? correctGame.WhoseTurn.ToString().ToLowerInvariant() : null);
                        replayMoves.Add(move);
                        replayPockets.Add(correctGame.GenerateJsonPocket());
                    }
                    i++;
                }

                Current.Game = gameConstructor.Construct(Current.Variant, response.FEN);
                response.Moves = Current.Game.GetValidMoves(Current.Game.WhoseTurn);
            }
            response.ReplayFENs = replayFens;
            response.ReplayChecks = replayChecks;
            response.ReplayMoves = replayMoves;
            response.ReplayPockets = replayPockets;
        }
    }
}
