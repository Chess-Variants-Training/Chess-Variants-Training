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

namespace AtomicChessPuzzles.Controllers
{
    public class PuzzleController : Controller
    {
        IPuzzlesBeingEditedRepository puzzlesBeingEdited;
        IPuzzleRepository puzzleRepository;
        IPuzzlesTrainingRepository puzzlesTrainingRepository;

        public PuzzleController(IPuzzlesBeingEditedRepository _puzzlesBeingEdited, IPuzzleRepository _puzzleRepository, IPuzzlesTrainingRepository _puzzlesTrainingRepository)
        {
            puzzlesBeingEdited = _puzzlesBeingEdited;
            puzzleRepository = _puzzleRepository;
            puzzlesTrainingRepository = _puzzlesTrainingRepository;
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
        public IActionResult SubmitPuzzle(string id, string solution)
        {
            Puzzle puzzle = puzzlesBeingEdited.Get(id);
            if (puzzle == null)
            {
                return Json(new { success = false, error = string.Format("The given puzzle (ID: {0}) cannot be published because it isn't being created.", id) });
            }
            puzzle.Solutions.Add(solution);
            puzzle.Author = HttpContext.Session.GetString("user") ?? "Anonymous";
            puzzle.Game = null;
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
        public IActionResult GetOneRandomly()
        {
            Puzzle puzzle = puzzleRepository.GetOneRandomly();
            if (puzzle != null)
            {
                return Json(new { success = true, id = puzzle.ID });
            }
            else
            {
                return Json(new { success = false });
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
            do
            {
                pdt.TrainingSessionId = trainingSessionId ?? Guid.NewGuid().ToString();
            } while (puzzlesTrainingRepository.ContainsTrainingSessionId(trainingSessionId));
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
            if (string.Compare(pdt.SolutionMovesToDo[0], origin + "-" + destination, true) != 0)
            {
                return Json(new { success = true, correct = -1, solution = pdt.Puzzle.Solutions[0] });
            }
            MoveType type = pdt.Puzzle.Game.ApplyMove(new Move(origin, destination, pdt.Puzzle.Game.WhoseTurn), false);
            if (type == MoveType.Invalid)
            {
                return Json(new { success = false, error = "Invalid move." });
            }
            pdt.SolutionMovesToDo.RemoveAt(0);
            if (pdt.SolutionMovesToDo.Count == 0)
            {
                return Json(new { success = true, correct = 1, solution = pdt.Puzzle.Solutions[0], fen = pdt.Puzzle.Game.GetFen() });
            }
            string fen = pdt.Puzzle.Game.GetFen();
            string moveToPlay = pdt.SolutionMovesToDo[0];
            string[] parts = moveToPlay.Split('-');
            pdt.Puzzle.Game.ApplyMove(new Move(parts[0], parts[1], pdt.Puzzle.Game.WhoseTurn), true);
            string fenAfterPlay = pdt.Puzzle.Game.GetFen();
            Dictionary<string, List<string>> dests = GetChessgroundDestsForMoveCollection(pdt.Puzzle.Game.GetValidMoves(pdt.Puzzle.Game.WhoseTurn));
            JsonResult result = Json(new { success = true, correct = 0, fen = fen, play = moveToPlay, fenAfterPlay = fenAfterPlay, dests = dests });
            pdt.SolutionMovesToDo.RemoveAt(0);
            if (pdt.SolutionMovesToDo.Count == 0)
            {
                result = Json(new { success = true, correct = 1, fen = fen, play = moveToPlay, fenAfterPlay = fenAfterPlay, dests = dests });
            }
            return result;
        }
    }
}
