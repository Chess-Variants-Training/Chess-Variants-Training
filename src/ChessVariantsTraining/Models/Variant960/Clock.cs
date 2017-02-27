using MongoDB.Bson.Serialization.Attributes;
using System.Diagnostics;

namespace ChessVariantsTraining.Models.Variant960
{
    public class Clock
    {
        double secondsLimit;
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
            secondsLimit = tc.InitialSeconds;
            SecondsLeftAfterLatestMove = secondsLimit;
        }

        public void Start()
        {
            stopwatch.Start();
        }

        public void Pause()
        {
            stopwatch.Stop();
        }

        public void AddIncrement()
        {
            secondsLimit += timeControl.Increment;
        }

        public void MoveMade()
        {
            Pause();
            AddIncrement();
            SecondsLeftAfterLatestMove = GetSecondsLeft();
        }

        public double GetSecondsLeft()
        {
            return secondsLimit - stopwatch.Elapsed.TotalSeconds;
        }
    }
}
