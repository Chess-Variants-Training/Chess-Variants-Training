using AtomicChessPuzzles.DbRepositories;
using AtomicChessPuzzles.MemoryRepositories;
using AtomicChessPuzzles.Models;
using ChessDotNet;
using ChessDotNet.Variants.Atomic;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AtomicChessPuzzles.Controllers
{
    public class PuzzleController : Controller
    {
        IPuzzlesBeingEditedRepository puzzlesBeingEdited;
        IPuzzleRepository puzzleRepository;
        IPuzzlesTrainingRepository puzzlesTrainingRepository;
        ICommentRepository commentRepository;
        ICommentVoteRepository commentVoteRepository;
        IUserRepository userRepository;

        public PuzzleController(IPuzzlesBeingEditedRepository _puzzlesBeingEdited, IPuzzleRepository _puzzleRepository,
            IPuzzlesTrainingRepository _puzzlesTrainingRepository, ICommentRepository _commentRepository, ICommentVoteRepository _commentVoteRepository,
            IUserRepository _userRepository)
        {
            puzzlesBeingEdited = _puzzlesBeingEdited;
            puzzleRepository = _puzzleRepository;
            puzzlesTrainingRepository = _puzzlesTrainingRepository;
            commentRepository = _commentRepository;
            commentVoteRepository = _commentVoteRepository;
            userRepository = _userRepository;
        }

        [Route("Puzzle")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("Puzzle/Editor")]
        public IActionResult Editor()
        {
            return View();
        }

        [HttpPost]
        [Route("Puzzle/Editor/RegisterPuzzleForEditing")]
        public IActionResult RegisterPuzzleForEditing(string fen)
        {
            AtomicChessGame game = new AtomicChessGame(fen);
            Puzzle puzzle = new Puzzle();
            puzzle.Game = game;
            puzzle.InitialFen = fen;
            puzzle.Solutions = new List<string>();
            do
            {
                puzzle.ID = Guid.NewGuid().ToString();
            } while (puzzlesBeingEdited.Contains(puzzle.ID));
            puzzlesBeingEdited.Add(puzzle);
            return Json(new { success = true, id = puzzle.ID });
        }

        [HttpGet]
        [ResponseCache(Duration = 0, NoStore = true)]
        [Route("Puzzle/Editor/GetValidMoves/{id}")]
        public IActionResult GetValidMoves(string id)
        {
            Puzzle puzzle = puzzlesBeingEdited.Get(id);
            if (puzzle == null)
            {
                return Json(new { success = false, error = "The given ID does not correspond to a puzzle." });
            }
            ReadOnlyCollection<Move> validMoves = puzzle.Game.GetValidMoves(puzzle.Game.WhoseTurn);
            Dictionary<string, List<string>> dests = GetChessgroundDestsForMoveCollection(validMoves);
            return Json(new { success = true, dests = dests, whoseturn = puzzle.Game.WhoseTurn.ToString().ToLowerInvariant() });
        }

        Dictionary<string, List<string>> GetChessgroundDestsForMoveCollection(ReadOnlyCollection<Move> moves)
        {
            Dictionary<string, List<string>> dests = new Dictionary<string, List<string>>();
            foreach (Move m in moves)
            {
                string origin = m.OriginalPosition.ToString().ToLowerInvariant();
                string destination = m.NewPosition.ToString().ToLowerInvariant();
                if (!dests.ContainsKey(origin))
                {
                    dests.Add(origin, new List<string>());
                }
                dests[origin].Add(destination);
            }
            return dests;
        }

        [HttpPost]
        [Route("Puzzle/Editor/SubmitMove")]
        public IActionResult SubmitMove(string id, string origin, string destination)
        {
            Puzzle puzzle = puzzlesBeingEdited.Get(id);
            if (puzzle == null)
            {
                return Json(new { success = false, error = "The given ID does not correspond to a puzzle." });
            }
            MoveType type = puzzle.Game.ApplyMove(new Move(origin, destination, puzzle.Game.WhoseTurn), false);
            if (type.HasFlag(MoveType.Invalid))
            {
                return Json(new { success = false, error = "The given move is invalid." });
            }
            return Json(new { success = true, fen = puzzle.Game.GetFen() });
        }

        [HttpPost]
        [Route("/Puzzle/Editor/Submit")]
        public IActionResult SubmitPuzzle(string id, string solution, string explanation)
        {
            Puzzle puzzle = puzzlesBeingEdited.Get(id);
            if (puzzle == null)
            {
                return Json(new { success = false, error = string.Format("The given puzzle (ID: {0}) cannot be published because it isn't being created.", id) });
            }
            puzzle.Solutions.Add(solution);
            puzzle.Author = HttpContext.Session.GetString("user") ?? "Anonymous";
            puzzle.Game = null;
            puzzle.ExplanationUnsafe = explanation;
            puzzle.Rating = new Rating(1500, 350, 0.06);
            if (puzzleRepository.Add(puzzle))
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, error = "Something went wrong." });
            }
        }

        [HttpGet]
        [Route("/Puzzle/Train")]
        public IActionResult Train()
        {
            return View();
        }

        [HttpGet]
        [Route("/Puzzle/Train/GetOneRandomly")]
        public IActionResult GetOneRandomly(string trainingSessionId = null)
        {
            List<string> toBeExcluded = new List<string>();
            if (HttpContext.Session.GetString("user") != null)
            {
                toBeExcluded = userRepository.FindByUsername(HttpContext.Session.GetString("user")).SolvedPuzzles;
            }
            else if (trainingSessionId != null)
            {
                toBeExcluded = puzzlesTrainingRepository.GetForTrainingSessionId(trainingSessionId).Select(x => x.Puzzle.ID).ToList();
            }
            Puzzle puzzle = puzzleRepository.GetOneRandomly(toBeExcluded);
            if (puzzle != null)
            {
                return Json(new { success = true, id = puzzle.ID });
            }
            else
            {
                return Json(new { success = false, error = "There are no more puzzles for you." });
            }
        }

        [HttpPost]
        [Route("/Puzzle/Train/Setup")]
        public IActionResult SetupTraining(string id, string trainingSessionId = null)
        {
            Puzzle puzzle = puzzleRepository.Get(id);
            if (puzzle == null)
            {
                return Json(new { success = false, error = "Puzzle not found." });
            }
            puzzle.Game = new AtomicChessGame(puzzle.InitialFen);
            PuzzleDuringTraining pdt = new PuzzleDuringTraining();
            pdt.Puzzle = puzzle;
            if (trainingSessionId == null)
            {
                do
                {
                    pdt.TrainingSessionId = Guid.NewGuid().ToString();
                } while (puzzlesTrainingRepository.ContainsTrainingSessionId(trainingSessionId));
            }
            else
            {
                pdt.TrainingSessionId = trainingSessionId;
            }
            pdt.SolutionMovesToDo = new List<string>(puzzle.Solutions[0].Split(' '));
            puzzlesTrainingRepository.Add(pdt);
            return Json(new
            {
                success = true,
                trainingSessionId = pdt.TrainingSessionId,
                author = pdt.Puzzle.Author,
                fen = pdt.Puzzle.InitialFen,
                dests = GetChessgroundDestsForMoveCollection(pdt.Puzzle.Game.GetValidMoves(pdt.Puzzle.Game.WhoseTurn)),
                whoseTurn = pdt.Puzzle.Game.WhoseTurn.ToString().ToLowerInvariant()
            });
        }

        [HttpPost]
        [Route("/Puzzle/Train/SubmitMove")]
        public IActionResult SubmitTrainingMove(string id, string trainingSessionId, string origin, string destination)
        {
            PuzzleDuringTraining pdt = puzzlesTrainingRepository.Get(id, trainingSessionId);
            MoveType type = pdt.Puzzle.Game.ApplyMove(new Move(origin, destination, pdt.Puzzle.Game.WhoseTurn), false);
            if (type == MoveType.Invalid)
            {
                return Json(new { success = false, error = "Invalid move." });
            }
            GameStatus status = pdt.Puzzle.Game.Status;
            string check = status.Event == GameEvent.Check || status.Event == GameEvent.Checkmate ? ChessUtilities.GetOpponentOf(status.PlayerWhoCausedEvent).ToString().ToLowerInvariant() : null;
            if (status.Event == GameEvent.Checkmate || status.Event == GameEvent.VariantEnd)
            {
                string loggedInUser = HttpContext.Session.GetString("user");
                if (loggedInUser != null)
                {
                    AdjustRating(loggedInUser, pdt.Puzzle.ID, true);
                }
                return Json(new { success = true, correct = 1, solution = pdt.Puzzle.Solutions[0], fen = pdt.Puzzle.Game.GetFen(), explanation = pdt.Puzzle.ExplanationSafe, check = check, rating = (int)pdt.Puzzle.Rating.Value });
            }
            if (string.Compare(pdt.SolutionMovesToDo[0], origin + "-" + destination, true) != 0)
            {
                string loggedInUser = HttpContext.Session.GetString("user");
                if (loggedInUser != null)
                {
                    AdjustRating(loggedInUser, pdt.Puzzle.ID, false);
                }
                return Json(new { success = true, correct = -1, solution = pdt.Puzzle.Solutions[0], explanation = pdt.Puzzle.ExplanationSafe, check = check, rating = (int)pdt.Puzzle.Rating.Value });
            }
            pdt.SolutionMovesToDo.RemoveAt(0);
            if (pdt.SolutionMovesToDo.Count == 0)
            {
                string loggedInUser = HttpContext.Session.GetString("user");
                if (loggedInUser != null)
                {
                    AdjustRating(loggedInUser, pdt.Puzzle.ID, true);
                }
                return Json(new { success = true, correct = 1, solution = pdt.Puzzle.Solutions[0], fen = pdt.Puzzle.Game.GetFen(), explanation = pdt.Puzzle.ExplanationSafe, check = check, rating = (int)pdt.Puzzle.Rating.Value });
            }
            string fen = pdt.Puzzle.Game.GetFen();
            string moveToPlay = pdt.SolutionMovesToDo[0];
            string[] parts = moveToPlay.Split('-');
            pdt.Puzzle.Game.ApplyMove(new Move(parts[0], parts[1], pdt.Puzzle.Game.WhoseTurn), true);
            string fenAfterPlay = pdt.Puzzle.Game.GetFen();
            GameStatus statusAfterAutoMove = pdt.Puzzle.Game.Status;
            string checkAfterAutoMove = statusAfterAutoMove.Event == GameEvent.Check || statusAfterAutoMove.Event == GameEvent.Checkmate ? ChessUtilities.GetOpponentOf(statusAfterAutoMove.PlayerWhoCausedEvent).ToString().ToLowerInvariant() : null;
            Dictionary<string, List<string>> dests = GetChessgroundDestsForMoveCollection(pdt.Puzzle.Game.GetValidMoves(pdt.Puzzle.Game.WhoseTurn));
            JsonResult result = Json(new { success = true, correct = 0, fen = fen, play = moveToPlay, fenAfterPlay = fenAfterPlay, dests = dests, checkAfterAutoMove = checkAfterAutoMove });
            pdt.SolutionMovesToDo.RemoveAt(0);
            if (pdt.SolutionMovesToDo.Count == 0)
            {
                string loggedInUser = HttpContext.Session.GetString("user");
                if (loggedInUser != null)
                {
                    AdjustRating(loggedInUser, pdt.Puzzle.ID, true);
                }
                result = Json(new { success = true, correct = 1, fen = fen, play = moveToPlay, fenAfterPlay = fenAfterPlay, dests = dests, explanation = pdt.Puzzle.ExplanationSafe, checkAfterAutoMove = checkAfterAutoMove, rating = (int)pdt.Puzzle.Rating.Value });
            }
            return result;
        }

        void AdjustRating(string userId, string puzzleId, bool correct)
        {
            // Glicko-2 library: https://github.com/MaartenStaa/glicko2-csharp
            User user = userRepository.FindByUsername(userId);
            Puzzle puzzle = puzzleRepository.Get(puzzleId);
            if (user.SolvedPuzzles.Contains(puzzle.ID))
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
        }

        [HttpPost]
        [Route("/Puzzle/Comment/PostComment", Name = "PostComment")]
        public IActionResult PostComment(string commentBody, string puzzleId)
        {
            Comment comment = new Comment(Guid.NewGuid().ToString(), HttpContext.Session.GetString("user") ?? "Anonymous", commentBody, null, puzzleId);
            bool success = commentRepository.Add(comment);
            if (success)
            {
                return Json(new { success = true, bodySanitized = comment.BodySanitized });
            }
            else
            {
                return Json(new { success = false, error = "Could not post comment." });
            }
        }

        [HttpGet]
        [Route("/Puzzle/Comment/ViewComments", Name = "ViewComments")]
        public IActionResult ViewComments(string puzzleId)
        {
            if (puzzleId == null)
            {
                return View("Comments", null);
            }
            List<Comment> comments = commentRepository.GetByPuzzle(puzzleId);
            ReadOnlyCollection<ViewModels.Comment> roComments = new ViewModels.CommentSorter(comments, commentVoteRepository).Ordered;
            Dictionary<string, VoteType> votesByCurrentUser = new Dictionary<string, VoteType>();
            if (HttpContext.Session.GetString("user") != null)
            {
                votesByCurrentUser = commentVoteRepository.VotesByUserOnThoseComments(HttpContext.Session.GetString("user"), comments.Select(x => x.ID).ToList());
            }
            Tuple<ReadOnlyCollection<ViewModels.Comment>, Dictionary<string, VoteType>> model = new Tuple<ReadOnlyCollection<ViewModels.Comment>, Dictionary<string, VoteType>>(roComments, votesByCurrentUser);
            return View("Comments", model);
        }

        [HttpPost]
        [Route("/Puzzle/Comment/Upvote")]
        public IActionResult Upvote(string commentId)
        {
            bool success = commentVoteRepository.Add(new CommentVote(VoteType.Upvote, HttpContext.Session.GetString("user") ?? "Anonymous", commentId));
            if (success)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, error = "Couldn't vote. Have you already voted?" });
            }
        }

        [HttpPost]
        [Route("/Puzzle/Comment/Downvote")]
        public IActionResult Downvote(string commentId)
        {
            bool success = commentVoteRepository.Add(new CommentVote(VoteType.Downvote, HttpContext.Session.GetString("user") ?? "Anonymous", commentId));
            if (success)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, error = "Couldn't vote. Have you already voted?" });
            }
        }

        [HttpPost]
        [Route("/Puzzle/Comment/UndoVote")]
        public IActionResult UndoVote(string commentId)
        {
            bool success = commentVoteRepository.Undo(HttpContext.Session.GetString("user") ?? "Anonymous", commentId);
            if (success)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, error = "Couldn't undo vote." });
            }
        }

        [HttpPost]
        [Route("/Puzzle/Comment/Reply")]
        public IActionResult Reply(string to, string body, string puzzleId)
        {
            Comment comment = new Comment(Guid.NewGuid().ToString(), HttpContext.Session.GetString("user") ?? "Anonymous", body, to, puzzleId);
            bool success = commentRepository.Add(comment);
            if (success)
            {
                return Json(new { success = true, bodySanitized = comment.BodySanitized });
            }
            else
            {
                return Json(new { success = false, error = "Could not post comment." });
            }
        }
    }
}
