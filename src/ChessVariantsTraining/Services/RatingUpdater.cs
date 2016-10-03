using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.Models;
using System;

namespace ChessVariantsTraining.Services
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

        public void AdjustRating(int userId, int puzzleId, bool correct, DateTime attemptStarted, DateTime attemptEnded, string variant)
        {
            // Glicko-2 library: https://github.com/MaartenStaa/glicko2-csharp
            User user = userRepository.FindById(userId);
            Puzzle puzzle = puzzleRepository.Get(puzzleId);
            if (user.SolvedPuzzles.Contains(puzzle.ID) || puzzle.InReview || puzzle.Author == user.ID || puzzle.Reviewers.Contains(user.ID))
            {
                return;
            }
            Glicko2.RatingCalculator calculator = new Glicko2.RatingCalculator();
            double oldUserRating = user.Ratings[variant].Value;
            double oldPuzzleRating = puzzle.Rating.Value;
            Glicko2.Rating userRating = new Glicko2.Rating(calculator, oldUserRating, user.Ratings[variant].RatingDeviation, user.Ratings[variant].Volatility);
            Glicko2.Rating puzzleRating = new Glicko2.Rating(calculator, oldPuzzleRating, puzzle.Rating.RatingDeviation, puzzle.Rating.Volatility);
            Glicko2.RatingPeriodResults results = new Glicko2.RatingPeriodResults();
            results.AddResult(correct ? userRating : puzzleRating, correct ? puzzleRating : userRating);
            calculator.UpdateRatings(results);
            double newUserRating = userRating.GetRating();
            user.Ratings[variant] = new Rating(newUserRating, userRating.GetRatingDeviation(), userRating.GetVolatility());
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

            RatingWithMetadata rwm = new RatingWithMetadata(user.Ratings[variant], attemptEnded, user.ID, variant);
            ratingRepository.Add(rwm);
        }
    }
}