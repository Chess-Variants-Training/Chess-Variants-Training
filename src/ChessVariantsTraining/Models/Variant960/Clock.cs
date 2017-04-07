using MongoDB.Bson.Serialization.Attributes;
using System;
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

        public Clock()
        {
            stopwatch = new Stopwatch();
        }

        public Clock(TimeControl tc) : this()
        {
            Increment = tc.Increment;
            SecondsLeftAfterLatestMove = tc.InitialSeconds != 0 ? tc.InitialSeconds : Math.Max(tc.Increment, 3);
        }

        public void Start()
        {
            stopwatch.Start();
        }

        public void Pause()
        {
            stopwatch.Stop();
        }

        public void End()
        {
            Pause();
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
            stopwatch.Reset();
        }

        public double GetSecondsLeft()
        {
            return SecondsLeftAfterLatestMove - stopwatch.Elapsed.TotalSeconds;
        }
    }
}
