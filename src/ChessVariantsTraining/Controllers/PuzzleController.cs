using ChessDotNet;
using ChessDotNet.Variants.Crazyhouse;
using ChessDotNet.Variants.ThreeCheck;
using ChessVariantsTraining.Attributes;
using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.Extensions;
using ChessVariantsTraining.MemoryRepositories;
using ChessVariantsTraining.Models;
using ChessVariantsTraining.Models.GeneratorIntegration;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

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
        IRandomProvider randomProvider;

        static readonly string[] supportedVariants = new string[] { "Atomic", "KingOfTheHill", "ThreeCheck", "Antichess", "Horde", "RacingKings", "Crazyhouse" };

        public PuzzleController(IPuzzlesBeingEditedRepository _puzzlesBeingEdited,
            IPuzzleRepository _puzzleRepository,
            IUserRepository _userRepository,
            IRatingUpdater _ratingUpdater,
            IMoveCollectionTransformer _movecollectionTransformer,
            IPuzzleTrainingSessionRepository _puzzleTrainingSessionRepository,
            ICounterRepository _counterRepository,
            IPersistentLoginHandler _loginHandler,
            IGameConstructor _gameConstructor,
            IRandomProvider _randomProvider) : base(_userRepository, _loginHandler)
        {
            puzzlesBeingEdited = _puzzlesBeingEdited;
            puzzleRepository = _puzzleRepository;
            ratingUpdater = _ratingUpdater;
            moveCollectionTransformer = _movecollectionTransformer;
            puzzleTrainingSessionRepository = _puzzleTrainingSessionRepository;
            counterRepository = _counterRepository;
            gameConstructor = _gameConstructor;
            randomProvider = _randomProvider;
        }

        [Route("/Puzzle")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("/Puzzle/{variant:supportedVariantOrMixed}")]
        public async Task<IActionResult> Train(string variant)
        {
            variant = Utilities.NormalizeVariantNameCapitalization(variant);
            ViewBag.LoggedIn = (await loginHandler.LoggedInUserIdAsync(HttpContext)).HasValue;
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
        public async Task<IActionResult> RegisterPuzzleForEditing(string fen, string variant, int checksByWhite, int checksByBlack)
        {
            fen += " - 0 1";
            variant = Utilities.NormalizeVariantNameCapitalization(variant);
            if (!Array.Exists(supportedVariants, x => x == variant))
            {
                return Json(new { success = false, error = "Unsupported variant." });
            }
            if (variant == "ThreeCheck")
            {
                if (checksByWhite > 2 || checksByWhite < 0 || checksByBlack > 2 || checksByBlack < 0)
                {
                    return Json(new { success = false, error = "Invalid amount of checks." });
                }

                fen += String.Format(" +{0}+{1}", checksByWhite, checksByBlack);
            }
            Puzzle possibleDuplicate = await puzzleRepository.FindByFenAndVariantAsync(fen, variant);
            if (possibleDuplicate != null && possibleDuplicate.Approved)
            {
                return Json(new { success = false, error = "Duplicate; same FEN and variant: " + Url.Action("TrainId", "Puzzle", new { id = possibleDuplicate.ID }) });
            }
            ChessGame game = gameConstructor.Construct(variant, fen);
            Puzzle puzzle = new Puzzle();

            Task<int?> aid = loginHandler.LoggedInUserIdAsync(HttpContext);

            puzzle.Game = game;
            puzzle.InitialFen = fen;
            puzzle.Variant = variant;
            puzzle.Solutions = new List<string>();
            do
            {
                puzzle.ID = Guid.NewGuid().GetHashCode();
            } while (puzzlesBeingEdited.Contains(puzzle.ID));
            puzzle.Author = (await aid).Value;
            puzzlesBeingEdited.Add(puzzle);
            return Json(new { success = true, id = puzzle.ID });
        }

        [HttpGet]
        [Route("/Puzzle/Editor/GetValidMoves/{id}")]
        [Restricted(true, UserRole.NONE)]
        public async Task<IActionResult> GetValidMoves(string id)
        {
            if (!int.TryParse(id, out int puzzleId))
            {
                return Json(new { success = false, error = "The given ID is invalid." });
            }

            Puzzle puzzle = puzzlesBeingEdited.Get(puzzleId);
            if (puzzle == null)
            {
                return Json(new { success = false, error = "The given ID does not correspond to a puzzle." });
            }
            if (puzzle.Author != (await loginHandler.LoggedInUserIdAsync(HttpContext)).Value)
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
            return Json(new { success = true, dests, whoseturn = puzzle.Game.WhoseTurn.ToString().ToLowerInvariant(), pocket = puzzle.Game.GenerateJsonPocket() });
        }

        [HttpPost]
        [Route("/Puzzle/Editor/SubmitMove")]
        [Restricted(true, UserRole.NONE)]
        public async Task<IActionResult> SubmitMove(string id, string origin, string destination, string promotion = null)
        {
            if (!int.TryParse(id, out int puzzleId))
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
            if (puzzle.Author != (await loginHandler.LoggedInUserIdAsync(HttpContext)).Value)
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

        [HttpPost]
        [Route("/Puzzle/Editor/SubmitDrop")]
        [Restricted(true, UserRole.NONE)]
        public async Task<IActionResult> SubmitDrop(string id, string role, string pos)
        {
            if (!int.TryParse(id, out int puzzleId))
            {
                return Json(new { success = false, error = "The given ID is invalid." });
            }

            Puzzle puzzle = puzzlesBeingEdited.Get(puzzleId);
            if (puzzle == null)
            {
                return Json(new { success = false, error = "The given ID doe snot correspond to a puzzle." });
            }
            if (puzzle.Author != (await loginHandler.LoggedInUserIdAsync(HttpContext)).Value)
            {
                return Json(new { success = false, error = "Only the puzzle author can access this right now." });
            }

            if (!(puzzle.Game is CrazyhouseChessGame))
            {
                return Json(new { success = false, error = "This is not a crazyhouse puzzle." });
            }

            Piece p = Utilities.GetByRole(role, puzzle.Game.WhoseTurn);
            if (p == null)
            {
                return Json(new { success = false, error = "Invalid drop piece." });
            }

            Drop drop = new Drop(p, new Position(pos), puzzle.Game.WhoseTurn);

            CrazyhouseChessGame zhGame = puzzle.Game as CrazyhouseChessGame;
            if (!zhGame.IsValidDrop(drop))
            {
                return Json(new { success = true, valid = false });
            }

            zhGame.ApplyDrop(drop, true);
            return Json(new { success = true, valid = true });
        }

        [HttpPost("/Puzzle/Editor/NewVariation")]
        [Restricted(true, UserRole.NONE)]
        public async Task<IActionResult> NewVariation(string id)
        {
            if (!int.TryParse(id, out int puzzleId))
            {
                return Json(new { success = false, error = "The given ID is invalid." });
            }

            Puzzle puzzle = puzzlesBeingEdited.Get(puzzleId);
            if (puzzle == null)
            {
                return Json(new { success = false, error = "The given ID does not correspond to a puzzle." });
            }
            if (puzzle.Author != (await loginHandler.LoggedInUserIdAsync(HttpContext)).Value)
            {
                return Json(new { success = false, error = "Only the puzzle author can access this right now." });
            }

            puzzle.Game = gameConstructor.Construct(puzzle.Variant, puzzle.InitialFen);
            return Json(new { success = true, fen = puzzle.InitialFen });
        }

        [HttpPost]
        [Route("/Puzzle/Editor/Submit")]
        [Restricted(true, UserRole.NONE)]
        public async Task<IActionResult> SubmitPuzzle(string id, string solution, string explanation)
        {
            if (!int.TryParse(id, out int puzzleId))
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
            if (puzzle.Author != (await loginHandler.LoggedInUserIdAsync(HttpContext)).Value)
            {
                return Json(new { success = false, error = "Only the puzzle author can access this right now." });
            }
            Puzzle possibleDuplicate = await puzzleRepository.FindByFenAndVariantAsync(puzzle.InitialFen, puzzle.Variant);
            if (possibleDuplicate != null && possibleDuplicate.Approved)
            {
                return Json(new { success = false, error = "Duplicate; same FEN and variant: " + Url.Action("TrainId", "Puzzle", new { id = possibleDuplicate.ID }) });
            }

            puzzle.Solutions = new List<string>(solution.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)));
            if (puzzle.Solutions.Count == 0)
            {
                return Json(new { success = false, error = "There are no accepted variations." });
            }
            Task<int> pid = counterRepository.GetAndIncreaseAsync(Counter.PUZZLE_ID);
            puzzle.Game = null;
            puzzle.ExplanationUnsafe = explanation;
            puzzle.Rating = new Rating(1500, 350, 0.06);
            puzzle.Reviewers = new List<int>();
            if (UserRole.HasAtLeastThePrivilegesOf((await loginHandler.LoggedInUserAsync(HttpContext)).Roles, UserRole.PUZZLE_REVIEWER))
            {
                puzzle.InReview = false;
                puzzle.Approved = true;
                puzzle.Reviewers.Add((await loginHandler.LoggedInUserIdAsync(HttpContext)).Value);
            }
            else
            {
                puzzle.InReview = true;
                puzzle.Approved = false;
            }
            puzzle.DateSubmittedUtc = DateTime.UtcNow;
            puzzle.ID = await pid;
            if (await puzzleRepository.AddAsync(puzzle))
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
        public async Task<IActionResult> TrainId(int id)
        {
            Puzzle p = await puzzleRepository.GetAsync(id);
            if (p == null)
            {
                return ViewResultForHttpError(HttpContext, new HttpErrors.NotFound("The given puzzle could not be found."));
            }
            ViewBag.Variant = p.Variant;
            return View("Train", p);
        }

        [HttpGet]
        [Route("/Puzzle/Train/GetOneRandomly/{variant:supportedVariantOrMixed}")]
        public async Task<IActionResult> GetOneRandomly(string variant, string trainingSessionId = null)
        {
            variant = Utilities.NormalizeVariantNameCapitalization(variant);
            List<int> toBeExcluded;
            double nearRating = randomProvider.RandomRating();
            int? userId = await loginHandler.LoggedInUserIdAsync(HttpContext);
            if (userId.HasValue)
            {
                User u = await userRepository.FindByIdAsync(userId.Value);
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
            Puzzle puzzle = await puzzleRepository.GetOneRandomlyAsync(toBeExcluded, variant, userId, nearRating);
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
        public async Task<IActionResult> SetupTraining(string id, string trainingSessionId = null)
        {
            if (!int.TryParse(id, out int puzzleId))
            {
                return Json(new { success = false, error = "Invalid puzzle ID." });
            }
            Puzzle puzzle = await puzzleRepository.GetAsync(puzzleId);
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
                author = (await userRepository.FindByIdAsync(session.Current.Author)).Username,
                fen = session.Current.InitialFen,
                dests = moveCollectionTransformer.GetChessgroundDestsForMoveCollection(session.Current.Game.GetValidMoves(session.Current.Game.WhoseTurn)),
                whoseTurn = session.Current.Game.WhoseTurn.ToString().ToLowerInvariant(),
                variant = puzzle.Variant,
                additionalInfo,
                authorUrl = Url.Action("Profile", "User", new { id = session.Current.Author }),
                pocket = session.Current.Game.GenerateJsonPocket(),
                check = session.Current.Game.IsInCheck(Player.White) ? "white" : (session.Current.Game.IsInCheck(Player.Black) ? "black" : null)
            });
        }

        async Task<IActionResult> JsonAfterMove(SubmittedMoveResponse response, PuzzleTrainingSession session)
        {
            dynamic jsonResp = new ExpandoObject();
            if (response.Correct == 1 || response.Correct == -1)
            {
                int? loggedInUser = await loginHandler.LoggedInUserIdAsync(HttpContext);
                if (loggedInUser.HasValue)
                {
                    await ratingUpdater.AdjustRatingAsync(loggedInUser.Value, session.Current.ID, response.Correct == 1, session.CurrentPuzzleStartedUtc.Value, session.CurrentPuzzleEndedUtc.Value, session.Current.Variant);
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
                jsonResp.replayPockets = response.ReplayPockets;
            }
            if (response.Pocket != null) jsonResp.pocket = response.Pocket;
            if (response.PocketAfterAutoMove != null) jsonResp.pocketAfterAutoMove = response.PocketAfterAutoMove;
            if (response.AnalysisUrl != null) jsonResp.analysisUrl = response.AnalysisUrl;
            return Json(jsonResp);
        }

        [HttpPost]
        [Route("/Puzzle/Train/SubmitMove")]
        public async Task<IActionResult> SubmitTrainingMove(string id, string trainingSessionId, string origin, string destination, string promotion = null)
        {
            PuzzleTrainingSession session = puzzleTrainingSessionRepository.Get(trainingSessionId);
            SubmittedMoveResponse response = session.ApplyMove(origin, destination, promotion);
            return await JsonAfterMove(response, session);
        }

        [HttpPost]
        [Route("/Puzzle/Train/SubmitDrop")]
        public async Task<IActionResult> SubmitTrainingDrop(string id, string trainingSessionId, string role, string pos)
        {
            PuzzleTrainingSession session = puzzleTrainingSessionRepository.Get(trainingSessionId);
            SubmittedMoveResponse response = session.ApplyDrop(role, pos);
            if (response.Success && response.Correct == SubmittedMoveResponse.INVALID_MOVE)
            {
                return Json(new { success = true, invalidDrop = true, pos });
            }
            return await JsonAfterMove(response, session);
        }

        [HttpPost]
        [Route("/Puzzle/Generation/Submit")]
        [Restricted(true, UserRole.GENERATOR, UserRole.BETA_GENERATOR)]
        public async Task<IActionResult> SubmitGeneratedPuzzle(string category, string last_pos, string last_move, string move_list, string variant)
        {
            Puzzle generated = new Puzzle();
            variant = Utilities.NormalizeVariantNameCapitalization(variant.Replace(" ", "").Replace("-", ""));
            generated.Variant = variant;
            generated.Rating = new Rating(1500, 350, 0.06);
            generated.ExplanationUnsafe = "Auto-generated puzzle. Category: " + category;
            generated.Author = (await loginHandler.LoggedInUserIdAsync(HttpContext)).Value;
            generated.Approved = (await loginHandler.LoggedInUserAsync(HttpContext)).Roles.Contains(UserRole.GENERATOR);
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

            Puzzle possibleDuplicate = await puzzleRepository.FindByFenAndVariantAsync(generated.InitialFen, generated.Variant);
            if (possibleDuplicate != null)
            {
                return Json(new { success = false, error = "This puzzle is a duplicate." });
            }

            Task<int> pid = counterRepository.GetAndIncreaseAsync(Counter.PUZZLE_ID);

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

            generated.ID = await pid;

            if (await puzzleRepository.AddAsync(generated))
            {
                return Json(new { success = true, id = generated.ID });
            }
            else
            {
                return Json(new { success = false, error = "Failure when inserting puzzle in database." });
            }
        }

        [HttpPost]
        [Route("/Puzzle/Zh-Generator/Submit")]
        [Restricted(true, UserRole.GENERATOR)]
        public async Task<IActionResult> SubmitGeneratedCrazyhousePuzzle(string json)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            GeneratedPuzzle gen = JsonConvert.DeserializeObject<GeneratedPuzzle>(json, settings);

            Puzzle puzzle = new Puzzle
            {
                Variant = "Crazyhouse",
                Rating = new Rating(1500, 350, 0.06),
                Author = (await loginHandler.LoggedInUserIdAsync(HttpContext)).Value,
                Approved = (await loginHandler.LoggedInUserAsync(HttpContext)).Roles.Contains(UserRole.GENERATOR)
            };
            puzzle.InReview = !puzzle.Approved;
            if (!puzzle.InReview)
            {
                puzzle.Reviewers = new List<int>() { puzzle.Author };
            }
            else
            {
                puzzle.Reviewers = new List<int>();
            }
            puzzle.DateSubmittedUtc = DateTime.UtcNow;

            string[] fenParts = gen.FEN.Split(' ');
            fenParts[4] = "0";
            fenParts[5] = "1";
            fenParts[0] = fenParts[0].TrimEnd(']').Replace('[', '/');
            puzzle.InitialFen = string.Join(" ", fenParts);

            try
            {
                CrazyhouseChessGame gameToTest = new CrazyhouseChessGame(puzzle.InitialFen);
            }
            catch
            {
                return Json(new { success = true, failure = "fen" });
            }

            Puzzle possibleDuplicate = await puzzleRepository.FindByFenAndVariantAsync(puzzle.InitialFen, puzzle.Variant);
            if (possibleDuplicate != null)
            {
                return Json(new { success = false, failure = "duplicate" });
            }

            Task<int> pid = counterRepository.GetAndIncreaseAsync(Counter.PUZZLE_ID);

            puzzle.ExplanationUnsafe = string.Format("Mate in {0}. (Slower mates won't be accepted.) Position from {1} - {2}, played on {3}.",
                gen.Depth,
                gen.White,
                gen.Black,
                gen.Site.Contains("lichess") ? "Lichess" : (gen.Site.Contains("FICS") ? "FICS": "an unknown server"));
            puzzle.Solutions = gen.FlattenSolution();

            puzzle.ID = await pid;

            if (await puzzleRepository.AddAsync(puzzle))
            {
                return Json(new { success = true, id = puzzle.ID });
            }
            else
            {
                return Json(new { success = false, failure = "database" });
            }
        }
    }
}
