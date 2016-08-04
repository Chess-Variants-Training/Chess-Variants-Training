using AtomicChessPuzzles.DbRepositories;
using AtomicChessPuzzles.Models;
using System;

namespace AtomicChessPuzzles.Services
{
    public class RatingUpdater : IRatingUpdater
    {
        IUserRepository userRepository;
        IPuzzleRepository puzzleRepository;
        IRatingRepository ratingRepository;

        public RatingUpdater(IUserRepository _userRepository, IPuzzleRepository _puzzleRepository, IRatingRepository _ratingRepository)
        {
            userRepository = _userRepository;
            puzzleRepository = _puzzleRepository;
            ratingRepository = _ratingRepository;
        }

        public void AdjustRating(string userId, string puzzleId, bool correct)
        {
            // Glicko-2 library: https://github.com/MaartenStaa/glicko2-csharp
            User user = userRepository.FindByUsername(userId);
            Puzzle puzzle = puzzleRepository.Get(puzzleId);
            if (user.SolvedPuzzles.Contains(puzzle.ID) || puzzle.InReview)
            {
                return;
            }
            Glicko2.RatingCalculator calculator = new Glicko2.RatingCalculator();
            Glicko2.Rating userRating = new Glicko2.Rating(calculator, user.Rating.Value, user.Rating.RatingDeviation, user.Rating.Volatility);
            Glicko2.Rating puzzleRating = new Glicko2.Rating(calculator, puzzle.Rating.Value, puzzle.Rating.RatingDeviation, puzzle.Rating.Volatility);
            Glicko2.RatingPeriodResults results = new Glicko2.RatingPeriodResults();
            results.AddResult(correct ? userRating : puzzleRating, correct ? puzzleRating : userRating);
            calculator.UpdateRatings(results);
            user.Rating = new Rating(userRating.GetRating(), userRating.GetRatingDeviation(), userRating.GetVolatility());
            user.SolvedPuzzles.Add(puzzle.ID);
            if (correct)
            {
                user.PuzzlesCorrect++;
            }
            else
            {
                user.PuzzlesWrong++;
            }
            userRepository.Update(user);
            puzzleRepository.UpdateRating(puzzle.ID, new Rating(puzzleRating.GetRating(), puzzleRating.GetRatingDeviation(), puzzleRating.GetVolatility()));

            RatingWithMetadata rwm = new RatingWithMetadata(user.Rating, DateTime.UtcNow, user.ID);
            ratingRepository.Add(rwm);
        }
    }
}