using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.MemoryRepositories;
using ChessVariantsTraining.Models;
using ChessVariantsTraining.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace ChessVariantsTraining.Controllers
{
    public class TimedTrainingController : Controller
    {
        ITimedTrainingScoreRepository timedTrainingRepository;
        IPositionRepository positionRepository;
        ITimedTrainingSessionRepository timedTrainingSessionRepository;
        ITimedTrainingScoreRepository timedTrainingScoreRepository;
        IMoveCollectionTransformer moveCollectionTransformer;
        IPersistentLoginHandler loginHandler;
        IGameConstructor gameConstructor;

        public TimedTrainingController(ITimedTrainingScoreRepository _timedTrainingRepository, IPositionRepository _positionRepository, ITimedTrainingSessionRepository _timedTrainingSessionRepository,
                                       ITimedTrainingScoreRepository _timedTrainingScoreRepository, IMoveCollectionTransformer _moveCollectionTransformer, IPersistentLoginHandler _loginHandler, IGameConstructor _gameConstructor)
        {
            timedTrainingRepository = _timedTrainingRepository;
            positionRepository = _positionRepository;
            timedTrainingSessionRepository = _timedTrainingSessionRepository;
            timedTrainingScoreRepository = _timedTrainingScoreRepository;
            moveCollectionTransformer = _moveCollectionTransformer;
            loginHandler = _loginHandler;
            gameConstructor = _gameConstructor;
        }

        IActionResult StartTimedTraining(string type, string variant)
        {
            string sessionId = Guid.NewGuid().ToString();
            DateTime startTime = DateTime.UtcNow;
            DateTime endTime = startTime + new TimeSpan(0, 1, 0);
            TimedTrainingSession session = new TimedTrainingSession(sessionId, startTime, endTime,
                                        loginHandler.LoggedInUserId(HttpContext), type, variant, gameConstructor);
            timedTrainingSessionRepository.Add(session);
            TrainingPosition randomPosition = positionRepository.GetRandom(type);
            session.SetPosition(randomPosition);
            return Json(new { success = true, sessionId = sessionId, seconds = 60, fen = randomPosition.FEN, color = session.AssociatedGame.WhoseTurn.ToString().ToLowerInvariant(),
                              dests = moveCollectionTransformer.GetChessgroundDestsForMoveCollection(session.AssociatedGame.GetValidMoves(session.AssociatedGame.WhoseTurn)), lastMove = session.CurrentLastMoveToDisplay });
        
        }

        [Route("/Timed-Training")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("/Timed-Training/{variant:regex(Atomic|Horde|KingOfTheHill|ThreeCheck)}/Mate-In-One")]
        public IActionResult MateInOneTrainingPage(string variant)
        {
            variant = Utilities.NormalizeVariantNameCapitalization(variant);
            List<TimedTrainingScore> latestScores = null;
            int? userId;
            if ((userId = loginHandler.LoggedInUserId(HttpContext)).HasValue)
            {
                latestScores = timedTrainingScoreRepository.GetLatestScores(userId.Value);
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
        public IActionResult ForcedCaptureTrainingPage()
        {
            List<TimedTrainingScore> latestScores = null;
            int? userId;
            if ((userId = loginHandler.LoggedInUserId(HttpContext)).HasValue)
            {
                latestScores = timedTrainingScoreRepository.GetLatestScores(userId.Value);
            }
            ViewBag.Description = "Find the forced capture!";
            ViewBag.Type = "Forced-Capture";
            ViewBag.Variant = "Antichess";
            return View("TimedTraining", latestScores);
        }


        [HttpPost]
        [Route("/Timed-Training/{variant:regex(Atomic|Horde|KingOfTheHill|ThreeCheck)}/Mate-In-One/Start")]
        public IActionResult StartMateInOneTraining(string variant)
        {
            variant = Utilities.NormalizeVariantNameCapitalization(variant);
            return StartTimedTraining("mateInOne" + variant, variant);
        }

        [HttpPost]
        [Route("/Timed-Training/Antichess/Forced-Capture/Start")]
        public IActionResult StartForcedCaptureTraining()
        {
            return StartTimedTraining("forcedCaptureAntichess", "Antichess");
        }

        [HttpPost]
        [Route("/Timed-Training/VerifyAndGetNext")]
        public IActionResult VerifyAndGetNext(string sessionId, string origin, string destination, string promotion = null)
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
                    timedTrainingScoreRepository.Add(session.Score);
                    session.RecordedInDb = true;
                }
                return Json(new { success = true, ended = true });
            }
            bool correctMove = session.VerifyMove(origin, destination, promotion);
            if (correctMove)
            {
                TrainingPosition randomPosition = positionRepository.GetRandom(session.Type);
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
        public IActionResult AcknowledgeEnd(string sessionId)
        {
            TimedTrainingSession session = timedTrainingSessionRepository.Get(sessionId);
            if (session == null)
            {
                return Json(new { success = false, error = "Training session ID not found." });
            }
            if (!session.RecordedInDb && session.Score.Owner.HasValue)
            {
                timedTrainingScoreRepository.Add(session.Score);
                session.RecordedInDb = true;
            }
            double score = session.Score.Score;
            timedTrainingSessionRepository.Remove(session.SessionID);
            return Json(new { success = true, score = score });
        }
    }
}