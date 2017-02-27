using MongoDB.Bson.Serialization.Attributes;
using System.Diagnostics;

namespace ChessVariantsTraining.Models.Variant960
{
    public class Clock
    {
        Stopwatch stopwatch;

        [BsonElement("secondsLeftAfterLatestMove")]
        public double SecondsLeftAfterLatestMove
        {
            get;
            set;
        }

        [BsonElement("increment")]
        public int Increment
        {
            get;
            set;
        }

        public Clock() { }

        public Clock(TimeControl tc)
        {
            Increment = tc.Increment;
            SecondsLeftAfterLatestMove = tc.InitialSeconds;
            stopwatch = new Stopwatch();
        }

        public void Start()
        {
            stopwatch.Start();
        }

        public void Pause()
        {
            stopwatch.Stop();
            stopwatch.Reset();
        }

        public void AddIncrement()
        {
            SecondsLeftAfterLatestMove += Increment;
        }

        public void MoveMade()
        {
            Pause();
            AddIncrement();
            SecondsLeftAfterLatestMove = GetSecondsLeft();
        }

        public double GetSecondsLeft()
        {
            return SecondsLeftAfterLatestMove - stopwatch.Elapsed.TotalSeconds;
        }
    }
}
