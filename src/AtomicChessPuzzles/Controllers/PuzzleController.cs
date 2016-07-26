using AtomicChessPuzzles.DbRepositories;
using AtomicChessPuzzles.MemoryRepositories;
using AtomicChessPuzzles.Models;
using AtomicChessPuzzles.Services;
using ChessDotNet;
using ChessDotNet.Pieces;
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
        IUserRepository userRepository;
        IRatingUpdater ratingUpdater;
        IMoveCollectionTransformer moveCollectionTransformer;

        public PuzzleController(IPuzzlesBeingEditedRepository _puzzlesBeingEdited, IPuzzleRepository _puzzleRepository,
            IPuzzlesTrainingRepository _puzzlesTrainingRepository, IUserRepository _userRepository, IRatingUpdater _ratingUpdater,
            IMoveCollectionTransformer _movecollectionTransformer)
        {
            puzzlesBeingEdited = _puzzlesBeingEdited;
            puzzleRepository = _puzzleRepository;
            puzzlesTrainingRepository = _puzzlesTrainingRepository;
            userRepository = _userRepository;
            ratingUpdater = _ratingUpdater;
            moveCollectionTransformer = _movecollectionTransformer;
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
            Dictionary<string, List<string>> dests = moveCollectionTransformer.GetChessgroundDestsForMoveCollection(validMoves);
            return Json(new { success = true, dests = dests, whoseturn = puzzle.Game.WhoseTurn.ToString().ToLowerInvariant() });
        }

        [HttpPost]
        [Route("Puzzle/Editor/SubmitMove")]
        public IActionResult SubmitMove(string id, string origin, string destination, string promotion = null)
        {
            Puzzle puzzle = puzzlesBeingEdited.Get(id);
            if (puzzle == null)
            {
                return Json(new { success = false, error = "The given ID does not correspond to a puzzle." });
            }
            Piece promotionPiece = null;
            if (promotion != null)
            {
                promotionPiece = Utilities.GetPromotionPieceFromName(promotion, puzzle.Game.WhoseTurn);
                if (promotionPiece == null)
                {
                    return Json(new { success = false, error = "Invalid promotion piece." });
                }
            }
            MoveType type = puzzle.Game.ApplyMove(new Move(origin, destination, puzzle.Game.WhoseTurn, promotionPiece), false);
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
            puzzle.Author = HttpContext.Session.GetString("username") ?? "Anonymous";
            puzzle.Game = null;
            puzzle.ExplanationUnsafe = explanation;
            puzzle.Rating = new Rating(1500, 350, 0.06);
            puzzle.InReview = true;
            puzzle.Approved = false;
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
        [Route("/p/{id}", Name = "TrainId")]
        public IActionResult TrainId(string id)
        {
            Puzzle p = puzzleRepository.Get(id);
            return View("Train", p);
        }

        [HttpGet]
        [Route("/Puzzle/Train/GetOneRandomly")]
        public IActionResult GetOneRandomly(string trainingSessionId = null)
        {
            List<string> toBeExcluded = new List<string>();
            double nearRating = 1500;
            if (HttpContext.Session.GetString("username") != null)
            {
                string userId = HttpContext.Session.GetString("username");
                User u = userRepository.FindByUsername(userId);
                toBeExcluded = u.SolvedPuzzles;
                nearRating = u.Rating.Value;
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
                dests = moveCollectionTransformer.GetChessgroundDestsForMoveCollection(pdt.Puzzle.Game.GetValidMoves(pdt.Puzzle.Game.WhoseTurn)),
                whoseTurn = pdt.Puzzle.Game.WhoseTurn.ToString().ToLowerInvariant()
            });
        }

        [HttpPost]
        [Route("/Puzzle/Train/SubmitMove")]
        public IActionResult SubmitTrainingMove(string id, string trainingSessionId, string origin, string destination, string promotion = null)
        {
            PuzzleDuringTraining pdt = puzzlesTrainingRepository.Get(id, trainingSessionId);
            Piece promotionPiece = null;
            if (promotion != null)
            {
                promotionPiece = Utilities.GetPromotionPieceFromName(promotion, pdt.Puzzle.Game.WhoseTurn);
                if (promotionPiece == null)
                {
                    return Json(new { success = false, error = "Invalid promotion piece." });
                }
            }
            MoveType type = pdt.Puzzle.Game.ApplyMove(new Move(origin, destination, pdt.Puzzle.Game.WhoseTurn, promotionPiece), false);
            if (type == MoveType.Invalid)
            {
                return Json(new { success = false, error = "Invalid move." });
            }
            string check = pdt.Puzzle.Game.IsInCheck(pdt.Puzzle.Game.WhoseTurn) ? pdt.Puzzle.Game.WhoseTurn.ToString().ToLowerInvariant() : null;
            if (pdt.Puzzle.Game.IsCheckmated(pdt.Puzzle.Game.WhoseTurn) || pdt.Puzzle.Game.KingIsGone(pdt.Puzzle.Game.WhoseTurn))
            {
                string loggedInUser = HttpContext.Session.GetString("userid");
                if (loggedInUser != null)
                {
                    ratingUpdater.AdjustRating(loggedInUser, pdt.Puzzle.ID, true);
                }
                return Json(new { success = true, correct = 1, solution = pdt.Puzzle.Solutions[0], fen = pdt.Puzzle.Game.GetFen(), explanation = pdt.Puzzle.ExplanationSafe, check = check, rating = (int)pdt.Puzzle.Rating.Value });
            }
            if (string.Compare(pdt.SolutionMovesToDo[0], origin + "-" + destination + (promotion != null ? "=" + char.ToUpperInvariant(promotionPiece.GetFenCharacter()) : ""), true) != 0)
            {
                string loggedInUser = HttpContext.Session.GetString("userid");
                if (loggedInUser != null)
                {
                    ratingUpdater.AdjustRating(loggedInUser, pdt.Puzzle.ID, false);
                }
                return Json(new { success = true, correct = -1, solution = pdt.Puzzle.Solutions[0], explanation = pdt.Puzzle.ExplanationSafe, check = check, rating = (int)pdt.Puzzle.Rating.Value });
            }
            pdt.SolutionMovesToDo.RemoveAt(0);
            if (pdt.SolutionMovesToDo.Count == 0)
            {
                string loggedInUser = HttpContext.Session.GetString("userid");
                if (loggedInUser != null)
                {
                    ratingUpdater.AdjustRating(loggedInUser, pdt.Puzzle.ID, true);
                }
                return Json(new { success = true, correct = 1, solution = pdt.Puzzle.Solutions[0], fen = pdt.Puzzle.Game.GetFen(), explanation = pdt.Puzzle.ExplanationSafe, check = check, rating = (int)pdt.Puzzle.Rating.Value });
            }
            string fen = pdt.Puzzle.Game.GetFen();
            string moveToPlay = pdt.SolutionMovesToDo[0];
            string[] parts = moveToPlay.Split('-');
            pdt.Puzzle.Game.ApplyMove(new Move(parts[0], parts[1], pdt.Puzzle.Game.WhoseTurn), true);
            string fenAfterPlay = pdt.Puzzle.Game.GetFen();
            string checkAfterAutoMove = pdt.Puzzle.Game.IsInCheck(pdt.Puzzle.Game.WhoseTurn) ? pdt.Puzzle.Game.WhoseTurn.ToString().ToLowerInvariant() : null;
            Dictionary<string, List<string>> dests = moveCollectionTransformer.GetChessgroundDestsForMoveCollection(pdt.Puzzle.Game.GetValidMoves(pdt.Puzzle.Game.WhoseTurn));
            JsonResult result = Json(new { success = true, correct = 0, fen = fen, play = moveToPlay, fenAfterPlay = fenAfterPlay, dests = dests, checkAfterAutoMove = checkAfterAutoMove });
            pdt.SolutionMovesToDo.RemoveAt(0);
            if (pdt.SolutionMovesToDo.Count == 0)
            {
                string loggedInUser = HttpContext.Session.GetString("userid");
                if (loggedInUser != null)
                {
                    ratingUpdater.AdjustRating(loggedInUser, pdt.Puzzle.ID, true);
                }
                result = Json(new { success = true, correct = 1, fen = fen, play = moveToPlay, fenAfterPlay = fenAfterPlay, dests = dests, explanation = pdt.Puzzle.ExplanationSafe, checkAfterAutoMove = checkAfterAutoMove, rating = (int)pdt.Puzzle.Rating.Value });
            }
            return result;
        }
    }
}
