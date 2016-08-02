using ChessDotNet;
using ChessDotNet.Pieces;
using ChessDotNet.Variants.Atomic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AtomicChessPuzzles.Models
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

        public AtomicChessGame Game
        {
            get;
            private set;
        }

        public bool WasAlreadyCheckmate
        {
            get;
            private set;
        }

        public EndgameTrainingSession(string sessionId, AtomicChessGame game)
        {
            SessionID = sessionId;
            Game = game;
            if (Game.IsInCheck(Player.Black))
            {
                Game = new AtomicChessGame(Game.GetFen().Replace(" w ", " b "));
                ReadOnlyCollection<Move> validMoves = Game.GetValidMoves(Player.Black);
                if (validMoves.Count == 0)
                {
                    WasAlreadyCheckmate = true;
                }
                else
                {
                    Game.ApplyMove(validMoves[rnd.Next(0, validMoves.Count)], true);
                    Game = new AtomicChessGame(string.Join(" ", Game.GetFen().Split(' ').Take(4)) + " 0 1");
                }
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

            if (Game.IsCheckmated(Game.WhoseTurn) || Game.KingIsGone(Game.WhoseTurn))
            {
                response.Correct = 1;
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
            Move chosen = null;
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
            Game.ApplyMove(chosen, true);
            response.CheckAfterAutoMove = Game.IsInCheck(Game.WhoseTurn) ? Game.WhoseTurn.ToString().ToLowerInvariant() : null;
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
