using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.MemoryRepositories;
using ChessVariantsTraining.Models;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessVariantsTraining.Controllers
{
    public class TimedTrainingController : CVTController
    {
        ITimedTrainingScoreRepository timedTrainingRepository;
        IPositionRepository positionRepository;
        ITimedTrainingSessionRepository timedTrainingSessionRepository;
        ITimedTrainingScoreRepository timedTrainingScoreRepository;
        IMoveCollectionTransformer moveCollectionTransformer;
        IGameConstructor gameConstructor;

        public TimedTrainingController(IUserRepository _userRepository,
            ITimedTrainingScoreRepository _timedTrainingRepository,
            IPositionRepository _positionRepository,
            ITimedTrainingSessionRepository _timedTrainingSessionRepository,
            ITimedTrainingScoreRepository _timedTrainingScoreRepository, 
            IMoveCollectionTransformer _moveCollectionTransformer,
            IPersistentLoginHandler _loginHandler,
            IGameConstructor _gameConstructor)
            : base(_userRepository, _loginHandler)
        {
            timedTrainingRepository = _timedTrainingRepository;
            positionRepository = _positionRepository;
            timedTrainingSessionRepository = _timedTrainingSessionRepository;
            timedTrainingScoreRepository = _timedTrainingScoreRepository;
            moveCollectionTransformer = _moveCollectionTransformer;
            loginHandler = _loginHandler;
            gameConstructor = _gameConstructor;
        }

        async Task<IActionResult> StartTimedTraining(string type, string variant)
        {
            string sessionId = Guid.NewGuid().ToString();
            DateTime startTime = DateTime.UtcNow;
            DateTime endTime = startTime + new TimeSpan(0, 1, 0);
            TimedTrainingSession session = new TimedTrainingSession(sessionId, startTime, endTime,
                                        await loginHandler.LoggedInUserIdAsync(HttpContext), type, variant, gameConstructor);
            timedTrainingSessionRepository.Add(session);
            TrainingPosition randomPosition = await positionRepository.GetRandomAsync(type);
            session.SetPosition(randomPosition);
            return Json(new { success = true, sessionId, seconds = 60, fen = randomPosition.FEN, color = session.AssociatedGame.WhoseTurn.ToString().ToLowerInvariant(),
                              dests = moveCollectionTransformer.GetChessgroundDestsForMoveCollection(session.AssociatedGame.GetValidMoves(session.AssociatedGame.WhoseTurn)), lastMove = session.CurrentLastMoveToDisplay });
        
        }

        [Route("/Timed-Training")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("/Timed-Training/{variant:regex(Atomic|Horde|KingOfTheHill|ThreeCheck)}/Mate-In-One")]
        public async Task<IActionResult> MateInOneTrainingPage(string variant)
        {
            variant = Utilities.NormalizeVariantNameCapitalization(variant);
            List<TimedTrainingScore> latestScores = null;
            int? userId;
            if ((userId = await loginHandler.LoggedInUserIdAsync(HttpContext)).HasValue)
            {
                latestScores = await timedTrainingScoreRepository.GetLatestScoresAsync(userId.Value, "mateInOne" + variant);
            }
            switch (variant)
            {
                case "Atomic":
                    ViewBag.Description = "Checkmate or explode the opponent's king!";
                    break;
                case "Horde":
                    ViewBag.Description = "If you're the white player, checkmate the black king! If you're the black player, destroy the horde!";
                    break;
                case "KingOfTheHill":
                    ViewBag.Description = "Checkmate the opponent's king or bring your king to the center!";
                    break;
                case "ThreeCheck":
                    ViewBag.Description = "Apply the third check to win the game!";
                    break;
            }
            ViewBag.Variant = variant;
            ViewBag.Type = "Mate-In-One";
            return View("TimedTraining", latestScores);
        }

        [Route("/Timed-Training/Antichess/Forced-Capture")]
        public async Task<IActionResult> ForcedCaptureTrainingPage()
        {
            List<TimedTrainingScore> latestScores = null;
            int? userId;
            if ((userId = await loginHandler.LoggedInUserIdAsync(HttpContext)).HasValue)
            {
                latestScores = await timedTrainingScoreRepository.GetLatestScoresAsync(userId.Value, "forcedCaptureAntichess");
            }
            ViewBag.Description = "Find the forced capture!";
            ViewBag.Type = "Forced-Capture";
            ViewBag.Variant = "Antichess";
            return View("TimedTraining", latestScores);
        }


        [HttpPost]
        [Route("/Timed-Training/{variant:regex(Atomic|Horde|KingOfTheHill|ThreeCheck)}/Mate-In-One/Start")]
        public async Task<IActionResult> StartMateInOneTraining(string variant)
        {
            variant = Utilities.NormalizeVariantNameCapitalization(variant);
            return await StartTimedTraining("mateInOne" + variant, variant);
        }

        [HttpPost]
        [Route("/Timed-Training/Antichess/Forced-Capture/Start")]
        public async Task<IActionResult> StartForcedCaptureTraining()
        {
            return await StartTimedTraining("forcedCaptureAntichess", "Antichess");
        }

        [HttpPost]
        [Route("/Timed-Training/VerifyAndGetNext")]
        public async Task<IActionResult> VerifyAndGetNext(string sessionId, string origin, string destination, string promotion = null)
        {
            TimedTrainingSession session = timedTrainingSessionRepository.Get(sessionId);
            if (session == null)
            {
                return Json(new { success = false, error = "Training session ID not found." });
            }
            if (session.Ended)
            {
                if (!session.RecordedInDb && session.Score.Owner.HasValue)
                {
                    await timedTrainingScoreRepository.AddAsync(session.Score);
                    session.RecordedInDb = true;
                }
                return Json(new { success = true, ended = true });
            }
            bool correctMove = session.VerifyMove(origin, destination, promotion);
            if (correctMove)
            {
                TrainingPosition randomPosition = await positionRepository.GetRandomAsync(session.Type);
                session.SetPosition(randomPosition);
            }
            else
            {
                session.RetryCurrentPosition();
            }
            return Json(new { success = true, fen = session.CurrentFen, color = session.AssociatedGame.WhoseTurn.ToString().ToLowerInvariant(),
                              dests = moveCollectionTransformer.GetChessgroundDestsForMoveCollection(session.AssociatedGame.GetValidMoves(session.AssociatedGame.WhoseTurn)),
                              currentScore = session.Score.Score, lastMove = session.CurrentLastMoveToDisplay });
        }

        [HttpPost]
        [Route("/Timed-Training/AcknowledgeEnd")]
        public async Task<IActionResult> AcknowledgeEnd(string sessionId)
        {
            TimedTrainingSession session = timedTrainingSessionRepository.Get(sessionId);
            if (session == null)
            {
                return Json(new { success = false, error = "Training session ID not found." });
            }
            if (!session.RecordedInDb && session.Score.Owner.HasValue)
            {
                await timedTrainingScoreRepository.AddAsync(session.Score);
                session.RecordedInDb = true;
            }
            double score = session.Score.Score;
            timedTrainingSessionRepository.Remove(session.SessionID);
            return Json(new { success = true, score });
        }
    }
}