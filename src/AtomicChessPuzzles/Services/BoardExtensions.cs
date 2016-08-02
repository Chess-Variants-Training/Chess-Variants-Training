using ChessDotNet;
using ChessDotNet.Pieces;
using System;
using System.Collections.Generic;

namespace AtomicChessPuzzles.Services
{
    public static class BoardExtensions
    {
        public static Piece[][] GenerateEmptyBoard()
        {
            return new Piece[8][]
            {
                new Piece[8] { null, null, null, null, null, null, null, null },
                new Piece[8] { null, null, null, null, null, null, null, null },
                new Piece[8] { null, null, null, null, null, null, null, null },
                new Piece[8] { null, null, null, null, null, null, null, null },
                new Piece[8] { null, null, null, null, null, null, null, null },
                new Piece[8] { null, null, null, null, null, null, null, null },
                new Piece[8] { null, null, null, null, null, null, null, null },
                new Piece[8] { null, null, null, null, null, null, null, null }
            };
        }

        static int GetRandomInt(int exclusiveUpperBound)
        {
            return Math.Abs(Guid.NewGuid().GetHashCode()) % exclusiveUpperBound;
        }

        static int[][] adjacentSquaresRelativePositions = new int[][]
        {
                new int[2] { -1, 0 },
                new int[2] { 1, 0  },
                new int[2] { 0, -1 },
                new int[2] { 0, 1 },
                new int[2] { 1, 1 },
                new int[2] { -1, -1 },
                new int[2] { 1, -1 },
                new int[2] { -1, 1 }
        };


        private static List<int[]> EmptyAdjacentSquares(Piece[][] board, int x, int y)
        {
            List<int[]> empty = new List<int[]>();
            for (int i = 0; i < adjacentSquaresRelativePositions.Length; i++)
            {
                int xr = adjacentSquaresRelativePositions[i][0];
                int yr = adjacentSquaresRelativePositions[i][1];
                int xa = x + xr;
                int ya = y + yr;
                if (xa >= 0 && xa < 8 && ya >= 0 && ya < 8)
                {
                    if (board[ya][xa] == null)
                    {
                        empty.Add(new int[2] { xa, ya });
                    }
                }
            }
            return empty;
        }

        public static Piece[][] AddAdjacentKings(this Piece[][] board)
        {
            int x, y;
            List<int[]> emptyAdjacentSquares;

            do
            {
                x = GetRandomInt(8);
                y = GetRandomInt(8);
                emptyAdjacentSquares = EmptyAdjacentSquares(board, x, y);
            } while (board[y][x] != null || emptyAdjacentSquares.Count == 0);

            board[y][x] = new King(Player.White);

            int[] blackKingCoordinate = emptyAdjacentSquares[GetRandomInt(emptyAdjacentSquares.Count)];
            board[blackKingCoordinate[1]][blackKingCoordinate[0]] = new King(Player.Black);

            return board;
        }

        static Piece[][] AddPiece(this Piece[][] board, Piece piece)
        {
            int x, y;

            do
            {
                x = GetRandomInt(8);
                y = GetRandomInt(8);
            } while (board[y][x] != null);

            board[y][x] = piece;
            return board;
        }

        public static Piece[][] AddWhiteQueen(this Piece[][] board)
        {
            board.AddPiece(new Queen(Player.White));
            return board;
        }

        public static Piece[][] AddWhiteRook(this Piece[][] board)
        {
            board.AddPiece(new Rook(Player.White));
            return board;
        }
    }
}
