using AtomicChessPuzzles.Models;
using System.Collections.Generic;
using System.Linq;

namespace AtomicChessPuzzles.MemoryRepositories
{
    public class PuzzleTrainingSessionRepository : IPuzzleTrainingSessionRepository
    {
        List<PuzzleTrainingSession> sessions = new List<PuzzleTrainingSession>();

        public void Add(PuzzleTrainingSession session)
        {
            sessions.Add(session);
        }

        public PuzzleTrainingSession Get(string sessionId)
        {
            return sessions.Where(x => x.SessionID == sessionId).FirstOrDefault();
        }

        public void Remove(string sessionId)
        {
            sessions.RemoveAll(x => x.SessionID == sessionId);
        }

        public bool ContainsTrainingSessionId(string sessionId)
        {
            return sessions.Any(x => x.SessionID == sessionId);
        }
    }
}
