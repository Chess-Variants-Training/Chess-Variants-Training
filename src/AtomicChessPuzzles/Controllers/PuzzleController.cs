using AtomicChessPuzzles.DbRepositories;
using AtomicChessPuzzles.MemoryRepositories;
using AtomicChessPuzzles.Models;
using AtomicChessPuzzles.Services;
using ChessDotNet;
using ChessDotNet.Variants.Atomic;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;

namespace AtomicChessPuzzles.Controllers
{
    public class PuzzleController : Controller
    {
        IPuzzlesBeingEditedRepository puzzlesBeingEdited;
        IPuzzleRepository puzzleRepository;
        IUserRepository userRepository;
        IRatingUpdater ratingUpdater;
        IMoveCollectionTransformer moveCollectionTransformer;
        IPuzzleTrainingSessionRepository puzzleTrainingSessionRepository;

        public PuzzleController(IPuzzlesBeingEditedRepository _puzzlesBeingEdited, IPuzzleRepository _puzzleRepository,
            IUserRepository _userRepository, IRatingUpdater _ratingUpdater,
            IMoveCollectionTransformer _movecollectionTransformer, IPuzzleTrainingSessionRepository _puzzleTrainingSessionRepository)
        {
            puzzlesBeingEdited = _puzzlesBeingEdited;
            puzzleRepository = _puzzleRepository;
            userRepository = _userRepository;
            ratingUpdater = _ratingUpdater;
            moveCollectionTransformer = _movecollectionTransformer;
            puzzleTrainingSessionRepository = _puzzleTrainingSessionRepository;
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
            List<string> toBeExcluded;
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
                toBeExcluded = puzzleTrainingSessionRepository.Get(trainingSessionId)?.PastPuzzleIds ?? new List<string>();
            }
            else
            {
                toBeExcluded = new List<string>();
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
            PuzzleTrainingSession session;
            if (trainingSessionId == null)
            {
                string g;
                do
                {
                    g = Guid.NewGuid().ToString();
                } while (puzzleTrainingSessionRepository.ContainsTrainingSessionId(g));
                session = new PuzzleTrainingSession(g);
                puzzleTrainingSessionRepository.Add(session);
            }
            else
            {
                session = puzzleTrainingSessionRepository.Get(trainingSessionId);
                if (session == null)
                {
                    return Json(new { success = false, error = "Puzzle training session ID not found." });
                }
            }
            session.Setup(puzzle);
            return Json(new
            {
                success = true,
                trainingSessionId = session.SessionID,
                author = session.Current.Author,
                fen = session.Current.InitialFen,
                dests = moveCollectionTransformer.GetChessgroundDestsForMoveCollection(session.Current.Game.GetValidMoves(session.Current.Game.WhoseTurn)),
                whoseTurn = session.Current.Game.WhoseTurn.ToString().ToLowerInvariant()
            });
        }

        [HttpPost]
        [Route("/Puzzle/Train/SubmitMove")]
        public IActionResult SubmitTrainingMove(string id, string trainingSessionId, string origin, string destination, string promotion = null)
        {
            PuzzleTrainingSession session = puzzleTrainingSessionRepository.Get(trainingSessionId);
            SubmittedMoveResponse response = session.ApplyMove(origin, destination, promotion);
            dynamic jsonResp = new ExpandoObject();
            if (response.Correct == 1 || response.Correct == -1)
            {
                string loggedInUser = HttpContext.Session.GetString("userid");
                if (loggedInUser != null)
                {
                    ratingUpdater.AdjustRating(loggedInUser, session.Current.ID, response.Correct == 1);
                }
                jsonResp.rating = (int)session.Current.Rating.Value;
            }
            jsonResp.success = response.Success;
            jsonResp.correct = response.Correct;
            jsonResp.check = response.Check;
            if (response.Error != null) jsonResp.error = response.Error;
            if (response.FEN != null) jsonResp.fen = response.FEN;
            if (response.ExplanationSafe != null) jsonResp.explanation = response.ExplanationSafe;
            if (response.Play != null)
            {
                jsonResp.play = response.Play;
                jsonResp.fenAfterPlay = response.FenAfterPlay;
                jsonResp.checkAfterAutoMove = response.CheckAfterAutoMove;
            }
            if (response.Moves != null) jsonResp.dests = moveCollectionTransformer.GetChessgroundDestsForMoveCollection(response.Moves);
            if (response.ReplayFENs != null)
            {
                jsonResp.replayFens = response.ReplayFENs;
                jsonResp.replayChecks = response.ReplayChecks;
                jsonResp.replayMoves = response.ReplayMoves;
            }
            return Json(jsonResp);
        }
    }
}
