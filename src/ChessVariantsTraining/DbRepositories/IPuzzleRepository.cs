using ChessVariantsTraining.Models;
using MongoDB.Driver;
using System.Collections.Generic;

namespace ChessVariantsTraining.DbRepositories
{
    public interface IPuzzleRepository
    {
        bool Add(Puzzle puzzle);

        Puzzle Get(int id);

        Puzzle GetOneRandomly(List<int> excludedIds, double nearRating = 1500);

        DeleteResult Remove(int id);

        DeleteResult RemoveAllBy(int author);

        bool UpdateRating(int id, Rating newRating);

        List<Puzzle> InReview();

        bool Approve(int id);

        bool Reject(int id);
    }
}
