using MongoDB.Bson.Serialization.Attributes;
using System.Diagnostics;

namespace ChessVariantsTraining.Models.Variant960
{
    public class Clock
    {
        Stopwatch stopwatch;
        TimeControl timeControl;

        [BsonElement("secondsLeftAfterLatestMove")]
        public double SecondsLeftAfterLatestMove
        {
            get;
            set;
        }

        public Clock() { }

        public Clock(TimeControl tc)
        {
            timeControl = tc;
            SecondsLeftAfterLatestMove = tc.InitialSeconds;
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
            SecondsLeftAfterLatestMove += timeControl.Increment;
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
