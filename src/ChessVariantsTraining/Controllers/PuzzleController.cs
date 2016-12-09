using ChessDotNet;
using ChessDotNet.Variants.Antichess;
using ChessDotNet.Variants.Atomic;
using ChessDotNet.Variants.Horde;
using ChessDotNet.Variants.KingOfTheHill;
using ChessDotNet.Variants.RacingKings;
using ChessDotNet.Variants.ThreeCheck;
using ChessVariantsTraining.Attributes;
using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.MemoryRepositories;
using ChessVariantsTraining.Models;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;

namespace ChessVariantsTraining.Controllers
{
    public class PuzzleController : CVTController
    {
        IPuzzlesBeingEditedRepository puzzlesBeingEdited;
        IPuzzleRepository puzzleRepository;
        IRatingUpdater ratingUpdater;
        IMoveCollectionTransformer moveCollectionTransformer;
        IPuzzleTrainingSessionRepository puzzleTrainingSessionRepository;
        ICounterRepository counterRepository;
        IGameConstructor gameConstructor;

        static readonly string[] supportedVariants = new string[] { "Atomic", "KingOfTheHill", "ThreeCheck", "Antichess", "Horde", "RacingKings" };

        public PuzzleController(IPuzzlesBeingEditedRepository _puzzlesBeingEdited,
            IPuzzleRepository _puzzleRepository,
            IUserRepository _userRepository,
            IRatingUpdater _ratingUpdater,
            IMoveCollectionTransformer _movecollectionTransformer,
            IPuzzleTrainingSessionRepository _puzzleTrainingSessionRepository,
            ICounterRepository _counterRepository,
            IPersistentLoginHandler _loginHandler,
            IGameConstructor _gameConstructor) : base(_userRepository, _loginHandler)
        {
            puzzlesBeingEdited = _puzzlesBeingEdited;
            puzzleRepository = _puzzleRepository;
            ratingUpdater = _ratingUpdater;
            moveCollectionTransformer = _movecollectionTransformer;
            puzzleTrainingSessionRepository = _puzzleTrainingSessionRepository;
            counterRepository = _counterRepository;
            gameConstructor = _gameConstructor;
        }

        [Route("/Puzzle")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("/Puzzle/{variant:supportedVariantOrMixed}")]
        public IActionResult Train(string variant)
        {
            variant = Utilities.NormalizeVariantNameCapitalization(variant);
            ViewBag.LoggedIn = loginHandler.LoggedInUserId(HttpContext).HasValue;
            ViewBag.Variant = variant;
            return View("Train");
        }

        [Route("/Puzzle/Editor")]
        [Restricted(true, UserRole.NONE)]
        public IActionResult Editor()
        {
            return View();
        }

        [HttpPost]
        [Route("/Puzzle/Editor/RegisterPuzzleForEditing")]
        [Restricted(true, UserRole.NONE)]
        public IActionResult RegisterPuzzleForEditing(string fen, string variant)
        {
            fen += " - 0 1";
            variant = Utilities.NormalizeVariantNameCapitalization(variant);
            if (!Array.Exists(supportedVariants, x => x == variant))
            {
                return Json(new { success = false, error = "Unsupported variant." });
            }
            Puzzle possibleDuplicate = puzzleRepository.FindByFenAndVariant(fen, variant);
            if (possibleDuplicate != null && possibleDuplicate.Approved)
            {
                return Json(new { success = false, error = "Duplicate; same FEN and variant: " + Url.Action("TrainId", "Puzzle", new { id = possibleDuplicate.ID }) });
            }
            ChessGame game = gameConstructor.Construct(variant, fen);
            Puzzle puzzle = new Puzzle();
            puzzle.Game = game;
            puzzle.InitialFen = fen;
            puzzle.Variant = variant;
            puzzle.Author = loginHandler.LoggedInUserId(HttpContext).Value;
            puzzle.Solutions = new List<string>();
            do
            {
                puzzle.ID = Guid.NewGuid().GetHashCode();
            } while (puzzlesBeingEdited.Contains(puzzle.ID));
            puzzlesBeingEdited.Add(puzzle);
            return Json(new { success = true, id = puzzle.ID });
        }

        [HttpGet]
        [Route("/Puzzle/Editor/GetValidMoves/{id}")]
        [Restricted(true, UserRole.NONE)]
        public IActionResult GetValidMoves(string id)
        {
            int puzzleId;
            if (!int.TryParse(id, out puzzleId))
            {
                return Json(new { success = false, error = "The given ID is invalid." });
            }

            Puzzle puzzle = puzzlesBeingEdited.Get(puzzleId);
            if (puzzle == null)
            {
                return Json(new { success = false, error = "The given ID does not correspond to a puzzle." });
            }
            if (puzzle.Author != loginHandler.LoggedInUserId(HttpContext).Value)
            {
                return Json(new { success = false, error = "Only the puzzle author can access this right now." });
            }

            ReadOnlyCollection<Move> validMoves;
            if (puzzle.Game.IsWinner(Player.White) || puzzle.Game.IsWinner(Player.Black))
            {
                validMoves = new ReadOnlyCollection<Move>(new List<Move>());
            }
            else
            {
                validMoves = puzzle.Game.GetValidMoves(puzzle.Game.WhoseTurn);
            }
            Dictionary<string, List<string>> dests = moveCollectionTransformer.GetChessgroundDestsForMoveCollection(validMoves);
            return Json(new { success = true, dests = dests, whoseturn = puzzle.Game.WhoseTurn.ToString().ToLowerInvariant() });
        }

        [HttpPost]
        [Route("/Puzzle/Editor/SubmitMove")]
        [Restricted(true, UserRole.NONE)]
        public IActionResult SubmitMove(string id, string origin, string destination, string promotion = null)
        {
            int puzzleId;
            if (!int.TryParse(id, out puzzleId))
            {
                return Json(new { success = false, error = "The given ID is invalid." });
            }
            if (promotion != null && promotion.Length != 1)
            {
                return Json(new { success = false, error = "Invalid 'promotion' parameter." });
            }

            Puzzle puzzle = puzzlesBeingEdited.Get(puzzleId);
            if (puzzle == null)
            {
                return Json(new { success = false, error = "The given ID does not correspond to a puzzle." });
            }
            if (puzzle.Author != loginHandler.LoggedInUserId(HttpContext).Value)
            {
                return Json(new { success = false, error = "Only the puzzle author can access this right now." });
            }

            MoveType type = puzzle.Game.ApplyMove(new Move(origin, destination, puzzle.Game.WhoseTurn, promotion?[0]), false);
            if (type.HasFlag(MoveType.Invalid))
            {
                return Json(new { success = false, error = "The given move is invalid." });
            }
            return Json(new { success = true, fen = puzzle.Game.GetFen() });
        }

        [HttpPost("/Puzzle/Editor/NewVariation")]
        [Restricted(true, UserRole.NONE)]
        public IActionResult NewVariation(string id)
        {
            int puzzleId;
            if (!int.TryParse(id, out puzzleId))
            {
                return Json(new { success = false, error = "The given ID is invalid." });
            }

            Puzzle puzzle = puzzlesBeingEdited.Get(puzzleId);
            if (puzzle == null)
            {
                return Json(new { success = false, error = "The given ID does not correspond to a puzzle." });
            }
            if (puzzle.Author != loginHandler.LoggedInUserId(HttpContext).Value)
            {
                return Json(new { success = false, error = "Only the puzzle author can access this right now." });
            }

            puzzle.Game = gameConstructor.Construct(puzzle.Variant, puzzle.InitialFen);
            return Json(new { success = true, fen = puzzle.InitialFen });
        }

        [HttpPost]
        [Route("/Puzzle/Editor/Submit")]
        [Restricted(true, UserRole.NONE)]
        public IActionResult SubmitPuzzle(string id, string solution, string explanation)
        {
            int puzzleId;
            if (!int.TryParse(id, out puzzleId))
            {
                return Json(new { success = false, error = "The given ID is invalid." });
            }

            if (string.IsNullOrWhiteSpace(solution))
            {
                return Json(new { success = false, error = "There are no accepted variations." });
            }

            Puzzle puzzle = puzzlesBeingEdited.Get(puzzleId);
            if (puzzle == null)
            {
                return Json(new { success = false, error = string.Format("The given puzzle (ID: {0}) cannot be published because it isn't being created.", id) });
            }
            if (puzzle.Author != loginHandler.LoggedInUserId(HttpContext).Value)
            {
                return Json(new { success = false, error = "Only the puzzle author can access this right now." });
            }
            Puzzle possibleDuplicate = puzzleRepository.FindByFenAndVariant(puzzle.InitialFen, puzzle.Variant);
            if (possibleDuplicate != null && possibleDuplicate.Approved)
            {
                return Json(new { success = false, error = "Duplicate; same FEN and variant: " + Url.Action("TrainId", "Puzzle", new { id = possibleDuplicate.ID }) });
            }

            puzzle.Solutions = new List<string>(solution.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)));
            if (puzzle.Solutions.Count == 0)
            {
                return Json(new { success = false, error = "There are no accepted variations." });
            }
            puzzle.Game = null;
            puzzle.ExplanationUnsafe = explanation;
            puzzle.Rating = new Rating(1500, 350, 0.06);
            puzzle.Reviewers = new List<int>();
            if (UserRole.HasAtLeastThePrivilegesOf(loginHandler.LoggedInUser(HttpContext).Roles, UserRole.PUZZLE_REVIEWER))
            {
                puzzle.InReview = false;
                puzzle.Approved = true;
                puzzle.Reviewers.Add(loginHandler.LoggedInUserId(HttpContext).Value);
            }
            else
            {
                puzzle.InReview = true;
                puzzle.Approved = false;
            }
            puzzle.DateSubmittedUtc = DateTime.UtcNow;
            puzzle.ID = counterRepository.GetAndIncrease(Counter.PUZZLE_ID);
            if (puzzleRepository.Add(puzzle))
            {
                return Json(new { success = true, link = Url.Action("TrainId", "Puzzle", new { id = puzzle.ID }) });
            }
            else
            {
                return Json(new { success = false, error = "Something went wrong." });
            }
        }

        [HttpGet]
        [Route("/Puzzle/{id:int}", Name = "TrainId")]
        public IActionResult TrainId(int id)
        {
            Puzzle p = puzzleRepository.Get(id);
            if (p == null)
            {
                return ViewResultForHttpError(HttpContext, new HttpErrors.NotFound("The given puzzle could not be found."));
            }
            ViewBag.Variant = p.Variant;
            return View("Train", p);
        }

        [HttpGet]
        [Route("/Puzzle/Train/GetOneRandomly/{variant:supportedVariantOrMixed}")]
        public IActionResult GetOneRandomly(string variant, string trainingSessionId = null)
        {
            variant = Utilities.NormalizeVariantNameCapitalization(variant);
            List<int> toBeExcluded;
            double nearRating = 1500;
            int? userId = loginHandler.LoggedInUserId(HttpContext);
            if (userId.HasValue)
            {
                User u = userRepository.FindById(userId.Value);
                toBeExcluded = u.SolvedPuzzles;
                if (variant != "Mixed")
                {
                    nearRating = u.Ratings[variant].Value;
                }
                else
                {
                    nearRating = u.Ratings.Average(x => x.Value.Value);
                }
            }
            else if (trainingSessionId != null)
            {
                toBeExcluded = puzzleTrainingSessionRepository.Get(trainingSessionId)?.PastPuzzleIds ?? new List<int>();
            }
            else
            {
                toBeExcluded = new List<int>();
            }
            Puzzle puzzle = puzzleRepository.GetOneRandomly(toBeExcluded, variant, loginHandler.LoggedInUserId(HttpContext));
            if (puzzle != null)
            {
                return Json(new { success = true, id = puzzle.ID });
            }
            else
            {
                return Json(new { success = true, allDone = true });
            }
        }

        [HttpPost]
        [Route("/Puzzle/Train/Setup")]
        public IActionResult SetupTraining(string id, string trainingSessionId = null)
        {
            int puzzleId;
            if (!int.TryParse(id, out puzzleId))
            {
                return Json(new { success = false, error = "Invalid puzzle ID." });
            }
            Puzzle puzzle = puzzleRepository.Get(puzzleId);
            if (puzzle == null)
            {
                return Json(new { success = false, error = "Puzzle not found." });
            }
            puzzle.Game = gameConstructor.Construct(puzzle.Variant, puzzle.InitialFen);
            PuzzleTrainingSession session;
            if (trainingSessionId == null)
            {
                string g;
                do
                {
                    g = Guid.NewGuid().ToString();
                } while (puzzleTrainingSessionRepository.ContainsTrainingSessionId(g));
                session = new PuzzleTrainingSession(g, gameConstructor);
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
            string additionalInfo = null;
            if (puzzle.Variant == "ThreeCheck")
            {
                ThreeCheckChessGame tccg = puzzle.Game as ThreeCheckChessGame;
                additionalInfo = string.Format("At the puzzle's initial position, white delivered {0} checks and black delivered {1} checks.", tccg.ChecksByWhite, tccg.ChecksByBlack);
            }
            return Json(new
            {
                success = true,
                trainingSessionId = session.SessionID,
                author = userRepository.FindById(session.Current.Author).Username,
                fen = session.Current.InitialFen,
                dests = moveCollectionTransformer.GetChessgroundDestsForMoveCollection(session.Current.Game.GetValidMoves(session.Current.Game.WhoseTurn)),
                whoseTurn = session.Current.Game.WhoseTurn.ToString().ToLowerInvariant(),
                variant = puzzle.Variant,
                additionalInfo = additionalInfo,
                authorUrl = Url.Action("Profile", "User", new { id = session.Current.Author })
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
                int? loggedInUser = loginHandler.LoggedInUserId(HttpContext);
                if (loggedInUser.HasValue)
                {
                    ratingUpdater.AdjustRating(loggedInUser.Value, session.Current.ID, response.Correct == 1, session.CurrentPuzzleStartedUtc.Value, session.CurrentPuzzleEndedUtc.Value, session.Current.Variant);
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

        [HttpPost]
        [Route("/Puzzle/Generation/Submit")]
        [Restricted(true, UserRole.GENERATOR, UserRole.BETA_GENERATOR)]
        public IActionResult SubmitGeneratedPuzzle(string category, string last_pos, string last_move, string move_list, string variant)
        {
            Puzzle generated = new Puzzle();
            variant = Utilities.NormalizeVariantNameCapitalization(variant.Replace(" ", "").Replace("-", ""));
            generated.Variant = variant;
            generated.Rating = new Rating(1500, 350, 0.06);
            generated.ExplanationUnsafe = "Auto-generated puzzle. Category: " + category;
            generated.Author = loginHandler.LoggedInUserId(HttpContext).Value;
            generated.Approved = loginHandler.LoggedInUser(HttpContext).Roles.Contains(UserRole.GENERATOR);
            generated.InReview = !generated.Approved;
            generated.Reviewers = new List<int>();
            generated.DateSubmittedUtc = DateTime.UtcNow;

            string[] lastPosParts = last_pos.Split(' ');
            if (lastPosParts.Length == 7)
            {
                string counter = lastPosParts[4];

                lastPosParts[4] = lastPosParts[5];
                lastPosParts[5] = lastPosParts[6];

                int[] counterParts = counter.Split('+').Select(int.Parse).ToArray();
                int whiteDelivered = 3 - counterParts[0];
                int blackDelivered = 3 - counterParts[1];

                lastPosParts[6] = string.Format("+{0}+{1}", whiteDelivered, blackDelivered);
                last_pos = string.Join(" ", lastPosParts);
            }

            ChessGame game = gameConstructor.Construct(generated.Variant, last_pos);

            MoveType moveType = game.ApplyMove(new Move(last_move.Substring(0, 2), last_move.Substring(2, 2), game.WhoseTurn, last_move.Length == 4 ? null : new char?(last_move[last_move.Length - 1])),
                false);
            if (moveType == MoveType.Invalid)
            {
                return Json(new { success = false, error = "Invalid last_move." });
            }

            string[] initialFenParts = game.GetFen().Split(' ');
            initialFenParts[4] = "0";
            initialFenParts[5] = "1";

            generated.InitialFen = string.Join(" ", initialFenParts);

            Puzzle possibleDuplicate = puzzleRepository.FindByFenAndVariant(generated.InitialFen, generated.Variant);
            if (possibleDuplicate != null)
            {
                return Json(new { success = false, error = "This puzzle is a duplicate." });
            }

            generated.Solutions = new List<string>()
            {
                string.Join(" ",
                move_list.Split(' ')
                  .Select(x => string.Concat(
                      x.Substring(0, 2),
                      "-",
                      x.Substring(2, 2),
                      x.Length == 4 ? "" : "=" + x[x.Length - 1].ToString()
                    )
                  ))
            };

            generated.ID = counterRepository.GetAndIncrease(Counter.PUZZLE_ID);

            if (puzzleRepository.Add(generated))
            {
                return Json(new { success = true, id = generated.ID });
            }
            else
            {
                return Json(new { success = false, error = "Failure when inserting puzzle in database." });
            }
        }
    }
}
