using AtomicChessPuzzles.DbRepositories;
using AtomicChessPuzzles.MemoryRepositories;
using AtomicChessPuzzles.Models;
using AtomicChessPuzzles.Services;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http;
using System;
using System.Collections.Generic;

namespace AtomicChessPuzzles.Controllers
{
    public class TimedTrainingController : Controller
    {
        ITimedTrainingScoreRepository timedTrainingRepository;
        IPositionRepository positionRepository;
        ITimedTrainingSessionRepository timedTrainingSessionRepository;
        ITimedTrainingScoreRepository timedTrainingScoreRepository;
        IMoveCollectionTransformer moveCollectionTransformer;

        public TimedTrainingController(ITimedTrainingScoreRepository _timedTrainingRepository, IPositionRepository _positionRepository, ITimedTrainingSessionRepository _timedTrainingSessionRepository,
                                       ITimedTrainingScoreRepository _timedTrainingScoreRepository, IMoveCollectionTransformer _moveCollectionTransformer)
        {
            timedTrainingRepository = _timedTrainingRepository;
            positionRepository = _positionRepository;
            timedTrainingSessionRepository = _timedTrainingSessionRepository;
            timedTrainingScoreRepository = _timedTrainingScoreRepository;
            moveCollectionTransformer = _moveCollectionTransformer;
        }
        [HttpGet]
        [Route("/Timed-Training/Mate-In-One")]
        public IActionResult TimedMateInOne()
        {
            List<TimedTrainingScore> latestScores = null;
            int? userId;
            if((userId = HttpContext.Session.GetInt32("userid")).HasValue)
            {
                latestScores = timedTrainingScoreRepository.GetLatestScores(userId.Value);
            }
            return View(latestScores);
        }

        [HttpPost]
        [Route("/Timed-Training/Mate-In-One/Start")]
        public IActionResult StartMateInOneTraining()
        {
            string sessionId = Guid.NewGuid().ToString();
            DateTime startTime = DateTime.UtcNow;
            DateTime endTime = startTime + new TimeSpan(0, 1, 0);
            TimedTrainingSession session = new TimedTrainingSession(sessionId, startTime, endTime,
                                        HttpContext.Session.GetInt32("userid"), "mateInOne");
            timedTrainingSessionRepository.Add(session);
            TrainingPosition randomPosition = positionRepository.GetRandomMateInOne();
            session.SetPosition(randomPosition);
            return Json(new { success = true, sessionId = sessionId, seconds = 60, fen = randomPosition.FEN, color = session.AssociatedGame.WhoseTurn.ToString().ToLowerInvariant(),
                              dests = moveCollectionTransformer.GetChessgroundDestsForMoveCollection(session.AssociatedGame.GetValidMoves(session.AssociatedGame.WhoseTurn)) });
        }

        [HttpPost]
        [Route("/Timed-Training/Mate-In-One/VerifyAndGetNext")]
        public IActionResult MateInOneVerifyAndGetNext(string sessionId, string origin, string destination, string promotion = null)
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
                TrainingPosition randomPosition = positionRepository.GetRandomMateInOne();
                session.SetPosition(randomPosition);
            }
            else
            {
                session.RetryCurrentPosition();
            }
            return Json(new { success = true, fen = session.CurrentFen, color = session.AssociatedGame.WhoseTurn.ToString().ToLowerInvariant(),
                              dests = moveCollectionTransformer.GetChessgroundDestsForMoveCollection(session.AssociatedGame.GetValidMoves(session.AssociatedGame.WhoseTurn)),
                              currentScore = session.Score.Score });
        }

        [HttpPost]
        [Route("/Timed-Training/Mate-In-One/AcknowledgeEnd")]
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