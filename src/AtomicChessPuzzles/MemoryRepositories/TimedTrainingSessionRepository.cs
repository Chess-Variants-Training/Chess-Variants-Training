using AtomicChessPuzzles.Models;
using ChessDotNet.Variants.Atomic;
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

        public void SetCurrentFen(string sessionId, string fen, AtomicChessGame associatedGame)
        {
            for (int i = 0; i < sessions.Count; i++)
            {
                if (sessions[i].SessionID == sessionId)
                {
                    sessions[i].CurrentFen = fen;
                    sessions[i].AssociatedGame = associatedGame;
                    break;
                }
            }
        }
    }
}