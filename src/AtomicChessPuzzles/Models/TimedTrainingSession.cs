using ChessDotNet.Variants.Atomic;
using System;

namespace AtomicChessPuzzles.Models
{
    public class TimedTrainingSession
    {
        public string SessionID { get; private set; }
        public DateTime StartedAt { get; private set; }
        public DateTime EndsAt { get; private set; }
        public string CurrentFen { get; set; }
        public AtomicChessGame AssociatedGame { get; set; }
        public bool Ended
        { 
            get
            {
                return DateTime.UtcNow >= EndsAt;
            }
        }
        public bool RecordedInDb { get; set; }
        public TimedTrainingScore Score { get; set; }


        public TimedTrainingSession(string sessionId, DateTime startedAt, DateTime endsAt, string owner, string type)
        {
            SessionID = sessionId;
            StartedAt = startedAt;
            EndsAt = endsAt;
            RecordedInDb = false;
            Score = new TimedTrainingScore(0, type, owner);
        }
    }
}