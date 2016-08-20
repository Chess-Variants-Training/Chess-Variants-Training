using ChessVariantsTraining.Models;
using System.Collections.Generic;
using System.Linq;

namespace ChessVariantsTraining.MemoryRepositories
{
    public class EndgameTrainingSessionRepository : IEndgameTrainingSessionRepository
    {
        List<EndgameTrainingSession> sessions = new List<EndgameTrainingSession>();

        public void Add(EndgameTrainingSession session)
        {
            sessions.Add(session);
        }

        public EndgameTrainingSession Get(string sessionId)
        {
            return sessions.FirstOrDefault(x => x.SessionID == sessionId);
        }

        public void Remove(string sessionId)
        {
            sessions.RemoveAll(x => x.SessionID == sessionId);
        }

        public bool Exists(string sessionId)
        {
            return sessions.Any(x => x.SessionID == sessionId);
        }
    }
}
