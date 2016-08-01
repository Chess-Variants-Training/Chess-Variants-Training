using ChessDotNet;
using System;
using System.Collections.Generic;
using ChessDotNet.Pieces;

namespace AtomicChessPuzzles.Services
{
    public class BoardGenerator : IBoardGenerator
    {
        Random rnd = new Random();

        public Piece[][] Generate(Piece[] required, bool adjacentKings)
        {
            List<Piece> requiredList = new List<Piece>(required);
            Piece[][] result = new Piece[8][]
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
            List<Tuple<int, int>> usedCoordinates = new List<Tuple<int, int>>();
            if (requiredList.Contains(new King(Player.White)) && requiredList.Contains(new King(Player.Black)))
            {
                int x = rnd.Next(0, 8);
                int y = rnd.Next(0, 8);
                usedCoordinates.Add(new Tuple<int, int>(x, y));

                result[y][x] = new King(Player.White);

                if (adjacentKings)
                {
                    List<int[]> relativePositions = new List<int[]>();
                    if (x != 0)
                    {
                        relativePositions.Add(new int[2] { -1, 0 });
                    }
                    if (x != 7)
                    {
                        relativePositions.Add(new int[2] { 1, 0 });
                    }
                    if (y != 0)
                    {
                        relativePositions.Add(new int[2] { 0, -1 });
                    }
                    if (y != 7)
                    {
                        relativePositions.Add(new int[2] { 0, 1 });
                    }
                    if (x != 0 && y != 0)
                    {
                        relativePositions.Add(new int[2] { -1, -1 });
                    }
                    if (x != 0 && y != 7)
                    {
                        relativePositions.Add(new int[2] { -1, 1 });
                    }
                    if (x != 7 && y != 0)
                    {
                        relativePositions.Add(new int[2] { 1, -1 });
                    }
                    if (x != 7 && y != 7)
                    {
                        relativePositions.Add(new int[2] { 1, 1 });
                    }
                    int[] relativePos = relativePositions[rnd.Next(0, relativePositions.Count)];
                    int x2 = x + relativePos[0];
                    int y2 = y + relativePos[1];
                    usedCoordinates.Add(new Tuple<int, int>(x2, y2));
                    result[y2][x2] = new King(Player.Black);
                    requiredList.Remove(new King(Player.White));
                    requiredList.Remove(new King(Player.Black));
                }
                else
                {
                    throw new NotImplementedException();
                    // will be implemented once there is a need
                }
            }

            foreach (Piece piece in requiredList)
            {
                Tuple<int, int> coordinate;
                do
                {
                    coordinate = new Tuple<int, int>(rnd.Next(0, 8), rnd.Next(0, 8));
                } while (usedCoordinates.Contains(coordinate));
                usedCoordinates.Add(coordinate);
                result[coordinate.Item2][coordinate.Item1] = piece;
            }

            return result;
        }
    }
}
