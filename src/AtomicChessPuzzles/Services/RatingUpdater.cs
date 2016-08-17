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
        IAttemptRepository attemptRepository;

        public RatingUpdater(IUserRepository _userRepository, IPuzzleRepository _puzzleRepository, IRatingRepository _ratingRepository, IAttemptRepository _attemptRepository)
        {
            userRepository = _userRepository;
            puzzleRepository = _puzzleRepository;
            ratingRepository = _ratingRepository;
            attemptRepository = _attemptRepository;
        }

        public void AdjustRating(string userId, string puzzleId, bool correct, DateTime attemptStarted, DateTime attemptEnded)
        {
            // Glicko-2 library: https://github.com/MaartenStaa/glicko2-csharp
            User user = userRepository.FindByUsername(userId);
            Puzzle puzzle = puzzleRepository.Get(puzzleId);
            if (user.SolvedPuzzles.Contains(puzzle.ID) || puzzle.InReview)
            {
                return;
            }
            Glicko2.RatingCalculator calculator = new Glicko2.RatingCalculator();
            double oldUserRating = user.Rating.Value;
            double oldPuzzleRating = puzzle.Rating.Value;
            Glicko2.Rating userRating = new Glicko2.Rating(calculator, oldUserRating, user.Rating.RatingDeviation, user.Rating.Volatility);
            Glicko2.Rating puzzleRating = new Glicko2.Rating(calculator, oldPuzzleRating, puzzle.Rating.RatingDeviation, puzzle.Rating.Volatility);
            Glicko2.RatingPeriodResults results = new Glicko2.RatingPeriodResults();
            results.AddResult(correct ? userRating : puzzleRating, correct ? puzzleRating : userRating);
            calculator.UpdateRatings(results);
            double newUserRating = userRating.GetRating();
            user.Rating = new Rating(newUserRating, userRating.GetRatingDeviation(), userRating.GetVolatility());
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
            double newPuzzleRating = puzzleRating.GetRating();
            puzzleRepository.UpdateRating(puzzle.ID, new Rating(newPuzzleRating, puzzleRating.GetRatingDeviation(), puzzleRating.GetVolatility()));


            Attempt attempt = new Attempt(userId, puzzleId, attemptStarted, attemptEnded, newUserRating - oldUserRating, newPuzzleRating - oldPuzzleRating, correct);
            attemptRepository.Add(attempt);

            RatingWithMetadata rwm = new RatingWithMetadata(user.Rating, attemptEnded, user.ID);
            ratingRepository.Add(rwm);
        }
    }
}