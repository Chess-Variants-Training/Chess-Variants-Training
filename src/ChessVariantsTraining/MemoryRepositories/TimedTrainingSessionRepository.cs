using ChessVariantsTraining.Configuration;
using ChessVariantsTraining.DbRepositories;
using ChessVariantsTraining.Models;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ChessVariantsTraining.MemoryRepositories
{
    public class TimedTrainingSessionRepository : ITimedTrainingSessionRepository
    {
        List<TimedTrainingSession> sessions = new List<TimedTrainingSession>();
        ITimedTrainingScoreRepository scoreRepository;
        bool shouldAutoAcknowledge = true;

        public TimedTrainingSessionRepository(ITimedTrainingScoreRepository _scoreRepository, IOptions<Settings> appSettings)
        {
            scoreRepository = _scoreRepository;
            Thread cleanUpThread = new Thread(AutoAcknowledgeOldSessions);
            cleanUpThread.Start(appSettings.Value.TimedTrainingSessionAutoAcknowledgerDelay);
        }

        public void Add(TimedTrainingSession session)
        {
            sessions.Add(session);
        }

        public TimedTrainingSession Get(string sessionId)
        {
            return sessions.Where(x => x.SessionID == sessionId).FirstOrDefault();
        }

        public void Remove(string sessionId)
        {
            sessions.RemoveAll(x => x.SessionID == sessionId);
        }

        void AutoAcknowledgeOldSessions(object m)
        {
            int minutes = (int)m;
            while (shouldAutoAcknowledge)
            {
                List<TimedTrainingSession> sessionsCopy = new List<TimedTrainingSession>(sessions);
                for (int i = 0; i < sessionsCopy.Count; i++)
                {
                    if (sessionsCopy[i].AutoAcknowledegable)
                    {
                        if (!sessionsCopy[i].RecordedInDb && sessionsCopy[i].Score.Owner.HasValue)
                        {
                            scoreRepository.Add(sessionsCopy[i].Score);
                        }
                        Remove(sessionsCopy[i].SessionID);
                    }
                }
                Thread.Sleep(minutes * 60 * 1000);
            }
        }
    }
}