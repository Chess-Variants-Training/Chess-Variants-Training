using System.Diagnostics;

namespace ChessVariantsTraining.Models.Variant960
{
    public class Clock
    {
        double secondsLimit;
        Stopwatch stopwatch;
        TimeControl timeControl;

        public Clock(TimeControl tc)
        {
            timeControl = tc;
            secondsLimit = tc.InitialSeconds;
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

        public double GetSecondsLeft()
        {
            return secondsLimit - stopwatch.Elapsed.TotalSeconds;
        }
    }
}
