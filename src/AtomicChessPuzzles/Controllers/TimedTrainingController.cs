using AtomicChessPuzzles.DbRepositories;
using AtomicChessPuzzles.MemoryRepositories;
using AtomicChessPuzzles.Models;
using ChessDotNet;
using ChessDotNet.Variants.Atomic;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http;
using System;

namespace AtomicChessPuzzles.Controllers
{
    public class TimedTrainingController : Controller
    {
        ITimedTrainingScoreRepository timedTrainingRepository;
        IPositionRepository positionRepository;
        ITimedTrainingSessionRepository timedTrainingSessionRepository;
        ITimedTrainingScoreRepository timedTrainingScoreRepository;

        public TimedTrainingController(ITimedTrainingScoreRepository _timedTrainingRepository, IPositionRepository _positionRepository,
                                       ITimedTrainingSessionRepository _timedTrainingSessionRepository, ITimedTrainingScoreRepository _timedTrainingScoreRepository)
        {
            timedTrainingRepository = _timedTrainingRepository;
            positionRepository = _positionRepository;
            timedTrainingSessionRepository = _timedTrainingSessionRepository;
            timedTrainingScoreRepository = _timedTrainingScoreRepository;
        }
        [HttpGet]
        [Route("/Puzzle/Train-Timed/Mate-In-One")]
        public IActionResult TimedMateInOne()
        {
            return View();
        }

        [HttpPost]
        [Route("/Puzzle/Train-Timed/Mate-In-One/Start")]
        public IActionResult StartMateInOneTraining()
        {
            string sessionId = Guid.NewGuid().ToString();
            DateTime startTime = DateTime.UtcNow;
            DateTime endTime = startTime + new TimeSpan(0, 1, 0);
            TimedTrainingSession session = new TimedTrainingSession(sessionId, startTime, endTime,
                                        (HttpContext.Session.GetString("userid") ?? "").ToLower(), "mateInOne");
            timedTrainingSessionRepository.Add(session);
            TrainingPosition randomPosition = positionRepository.GetRandomMateInOne();
            AtomicChessGame associatedGame = new AtomicChessGame(randomPosition.FEN);
            timedTrainingSessionRepository.SetCurrentFen(sessionId, randomPosition.FEN, associatedGame);
            return Json(new { success = true, sessionId = sessionId, seconds = 60, fen = randomPosition.FEN, color = associatedGame.WhoseTurn.ToString().ToLowerInvariant(),
                              dests = Utilities.GetChessgroundDestsForMoveCollection(associatedGame.GetValidMoves(associatedGame.WhoseTurn)) });
        }

        [HttpPost]
        [Route("/Puzzle/Train-Timed/Mate-In-One/VerifyAndGetNext")]
        public IActionResult MateInOneVerifyAndGetNext(string sessionId, string origin, string destination)
        {
            TimedTrainingSession session = timedTrainingSessionRepository.Get(sessionId);
            if (session == null)
            {
                return Json(new { success = false, error = "Training session ID not found." });
            }
            if (session.Ended)
            {
                if (!session.RecordedInDb && !string.IsNullOrEmpty(session.Score.Owner))
                {
                    timedTrainingScoreRepository.Add(session.Score);
                    session.RecordedInDb = true;
                }
                return Json(new { success = true, ended = true });
            }
            bool correctMove = false;
            MoveType moveType = session.AssociatedGame.ApplyMove(new Move(origin, destination, session.AssociatedGame.WhoseTurn), false);
            if (moveType != MoveType.Invalid)
            {
                GameEvent gameEvent = session.AssociatedGame.Status.Event;
                correctMove = gameEvent == GameEvent.Checkmate || gameEvent == GameEvent.VariantEnd;
            }
            if (correctMove)
            {
                session.Score.Score++;
            }
            TrainingPosition randomPosition = positionRepository.GetRandomMateInOne();
            AtomicChessGame associatedGame = new AtomicChessGame(randomPosition.FEN);
            timedTrainingSessionRepository.SetCurrentFen(sessionId, randomPosition.FEN, associatedGame);
            return Json(new { success = true, fen = randomPosition.FEN, color = associatedGame.WhoseTurn.ToString().ToLowerInvariant(),
                              dests = Utilities.GetChessgroundDestsForMoveCollection(associatedGame.GetValidMoves(associatedGame.WhoseTurn)),
                              correct = correctMove });
        }

        [HttpPost]
        [Route("/Puzzle/Train-Timed/Mate-In-One/AcknowledgeEnd")]
        public IActionResult AcknowledgeEnd(string sessionId)
        {
            TimedTrainingSession session = timedTrainingSessionRepository.Get(sessionId);
            if (session == null)
            {
                return Json(new { success = false, error = "Training session ID not found." });
            }
            if (!session.RecordedInDb && !string.IsNullOrEmpty(session.Score.Owner))
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