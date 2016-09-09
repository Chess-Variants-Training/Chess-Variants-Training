using ChessDotNet;
using ChessDotNet.Pieces;
using ChessDotNet.Variants.Antichess;
using ChessDotNet.Variants.Atomic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ChessVariantsTraining.Models
{
    public class EndgameTrainingSession
    {
        Random rnd = new Random();

        public string SessionID
        {
            get;
            private set;
        }

        public string InitialFEN
        {
            get;
            private set;
        }

        public ChessGame Game
        {
            get;
            private set;
        }

        public bool WasAlreadyLost
        {
            get;
            private set;
        }

        public EndgameTrainingSession(string sessionId, ChessGame game)
        {
            SessionID = sessionId;
            Game = game;
            if (Game is AtomicChessGame && Game.IsInCheck(Player.Black))
            {
                Game = new AtomicChessGame(Game.GetFen().Replace(" w ", " b "));
                ReadOnlyCollection<Move> validMoves = Game.GetValidMoves(Player.Black);
                if (validMoves.Count == 0)
                {
                    WasAlreadyLost = true;
                }
                else
                {
                    Game.ApplyMove(validMoves[rnd.Next(0, validMoves.Count)], true);
                    Game = new AtomicChessGame(string.Join(" ", Game.GetFen().Split(' ').Take(4)) + " 0 1");
                }
            }
            ReadOnlyCollection<Move> antiValidMoves;
            if (Game is AntichessGame && (antiValidMoves = Game.GetValidMoves(Player.White)).Count == 1 && Game.GetPieceAt(antiValidMoves[0].NewPosition) != null)
            {
                WasAlreadyLost = true;
            }
            InitialFEN = Game.GetFen();
        }

        public SubmittedMoveResponse SubmitMove(Move move)
        {
            SubmittedMoveResponse response = new SubmittedMoveResponse();
            MoveType type = Game.ApplyMove(move, false);
            if (type == MoveType.Invalid)
            {
                response.Success = false;
                response.Error = "Invalid move.";
                response.Correct = -3;
                return response;
            }
            response.Success = true;
            response.Check = Game.IsInCheck(Game.WhoseTurn) ? Game.WhoseTurn.ToString().ToLowerInvariant() : null;
            response.FEN = Game.GetFen();

            if (Game.IsWinner(ChessUtilities.GetOpponentOf(Game.WhoseTurn)))
            {
                response.Correct = 1;
                return response;
            }
            else if (Game.IsWinner(Game.WhoseTurn))
            {
                response.Correct = -3;
                return response;
            }
            else if (Game.DrawCanBeClaimed)
            {
                response.Correct = -1;
                return response;
            }
            else if (Game.IsStalemated(Game.WhoseTurn))
            {
                response.Correct = -2;
                return response;
            }

            response.Correct = 0;

            Move chosen = null;
            if (Game is AtomicChessGame)
            {
                Position whiteKing = null;
                Position blackKing = null;
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 1; j < 9; j++)
                    {
                        if (Game.GetPieceAt((File)i, j) == new King(Player.White))
                        {
                            whiteKing = new Position((File)i, j);
                        }

                        if (Game.GetPieceAt((File)i, j) == new King(Player.Black))
                        {
                            blackKing = new Position((File)i, j);
                        }

                        if (whiteKing != null && blackKing != null)
                        {
                            break;
                        }
                    }
                }

                ReadOnlyCollection<Move> validKingMoves = Game.GetValidMoves(blackKing);
                List<Move> validKingMovesToAdjacentKings = new List<Move>();
                foreach (Move validMove in validKingMoves)
                {
                    PositionDistance distance = new PositionDistance(whiteKing, validMove.NewPosition);
                    if (distance.DistanceX <= 1 && distance.DistanceY <= 1)
                    {
                        validKingMovesToAdjacentKings.Add(validMove);
                    }
                }
                if (validKingMovesToAdjacentKings.Count > 0)
                {
                    chosen = validKingMovesToAdjacentKings[rnd.Next(0, validKingMovesToAdjacentKings.Count)];
                }
                else
                {
                    chosen = validKingMoves[rnd.Next(0, validKingMoves.Count)];
                }
            }
            else // (Game is AntichessGame)
            {
                ReadOnlyCollection<Move> validMovesBlack = Game.GetValidMoves(Player.Black);
                foreach (Move validMove in validMovesBlack)
                {
                    AntichessGame copy = new AntichessGame(Game.GetFen());
                    copy.ApplyMove(validMove, true);
                    ReadOnlyCollection<Move> validMovesWhite = copy.GetValidMoves(Player.White);
                    if (validMovesWhite.Any(x => copy.GetPieceAt(x.NewPosition) != null))
                    {
                        chosen = validMove;
                        break;
                    }
                    if (chosen == null)
                    {
                        bool thisOne = true;
                        foreach (Move vmWhite in validMovesWhite)
                        {
                            AntichessGame copy2 = new AntichessGame(copy.GetFen());
                            copy2.ApplyMove(vmWhite, true);
                            ReadOnlyCollection<Move> validMovesBlackStep2 = copy2.GetValidMoves(Player.Black);
                            if (validMovesBlackStep2.Any(x => copy2.GetPieceAt(x.NewPosition) != null))
                            {
                                thisOne = false;
                            }
                        }
                        if (thisOne)
                        {
                            chosen = validMove;
                        }
                    }
                }
                if (chosen == null)
                {
                    chosen = validMovesBlack[rnd.Next(0, validMovesBlack.Count)];
                }
            }
            Game.ApplyMove(chosen, true);
            response.CheckAfterAutoMove = Game.IsInCheck(Game.WhoseTurn) ? Game.WhoseTurn.ToString().ToLowerInvariant() : null;
            response.WinAfterAutoMove = Game.IsWinner(Game.WhoseTurn);
            response.FenAfterPlay = Game.GetFen();
            response.Moves = Game.GetValidMoves(Game.WhoseTurn);
            response.LastMove = new string[2] { chosen.OriginalPosition.ToString().ToLowerInvariant(), chosen.NewPosition.ToString().ToLowerInvariant() };
            if (Game.DrawCanBeClaimed)
            {
                response.DrawAfterAutoMove = true;
            }
            else
            {
                response.DrawAfterAutoMove = false;
            }

            return response;
        }
    }
}
