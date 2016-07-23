using AtomicChessPuzzles.Models;
using System.Collections.Generic;
using System.Linq;

namespace AtomicChessPuzzles.MemoryRepositories
{
    public class TimedTrainingSessionRepository : ITimedTrainingSessionRepository
    {
        List<TimedTrainingSession> sessions = new List<TimedTrainingSession>();

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
    }
}