using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessVariantsTraining.Models
{
    public class TimedTrainingChartData
    {
        public List<string> Labels
        {
            get;
            private set;
        }

        public Dictionary<string, List<int?>> Scores
        {
            get;
            private set;
        }

        public TimedTrainingChartData(IEnumerable<TimedTrainingScore> scores, bool fullDateTimeOnLabel)
        {
            IEnumerable<IGrouping<DateTime, TimedTrainingScore>> groupedByDatetime = scores.GroupBy(x => x.DateRecordedUtc);
            List<Element> elements = new List<Element>();
            foreach (IGrouping<DateTime, TimedTrainingScore> group in groupedByDatetime)
            {
                int? atomic = null;
                int? koth = null;
                int? threeCheck = null;
                int? antichess = null;
                int? horde = null;
                foreach (TimedTrainingScore score in group)
                {
                    switch (score.Type)
                    {
                        case "mateInOneAtomic":
                            atomic = (int)score.Score;
                            break;
                        case "mateInOneKingOfTheHill":
                            koth = (int)score.Score;
                            break;
                        case "mateInOneThreeCheck":
                            threeCheck = (int)score.Score;
                            break;
                        case "forcedCaptureAntichess":
                            antichess = (int)score.Score;
                            break;
                        case "mateInOneHorde":
                            horde = (int)score.Score;
                            break;
                    }
                }
                DateTime timestamp = group.Key;
                string label = fullDateTimeOnLabel ? timestamp.ToString() : timestamp.ToString("D");
                elements.Add(new Element(timestamp, label, atomic, koth, threeCheck, antichess, horde));
            }

            IEnumerable<Element> ordered = elements.OrderBy(x => x.Timestamp);
            List<int?> atomicScores = new List<int?>();
            List<int?> kothScores = new List<int?>();
            List<int?> threeCheckScores = new List<int?>();
            List<int?> antichessScores = new List<int?>();
            List<int?> hordeScores = new List<int?>();
            Labels = new List<string>();
            foreach (Element elem in ordered)
            {
                Labels.Add(elem.Label);
                atomicScores.Add(elem.AtomicMateInOneScore);
                kothScores.Add(elem.KothMateInOneScore);
                threeCheckScores.Add(elem.ThreeCheckMateInOneScore);
                antichessScores.Add(elem.AntichessForcedCaptureScore);
                hordeScores.Add(elem.HordeMateInOneScore);
            }
            Scores = new Dictionary<string, List<int?>>()
            {
                { "Antichess (forced capture)", antichessScores },
                { "Atomic (mate in one)", atomicScores },
                { "Horde (mate in one)", hordeScores },
                { "King of the Hill (mate in one)", kothScores },
                { "Three-check (third check)", threeCheckScores }
            };
        }

        private class Element
        {
            public DateTime Timestamp { get; set; }
            public string Label { get; set; }
            public int? AtomicMateInOneScore { get; set; }
            public int? KothMateInOneScore { get; set; }
            public int? ThreeCheckMateInOneScore { get; set; }
            public int? AntichessForcedCaptureScore { get; set; }
            public int? HordeMateInOneScore { get; set; }

            public Element(DateTime timestamp, string label, int? atomicMateInOneScore, int? kothMateInOneScore, int? threeCheckMateInOneScore, int? antichessForcedCaptureScore, int? hordeMateInOneScore)
            {
                Timestamp = timestamp;
                Label = label;
                AtomicMateInOneScore = atomicMateInOneScore;
                KothMateInOneScore = kothMateInOneScore;
                ThreeCheckMateInOneScore = threeCheckMateInOneScore;
                AntichessForcedCaptureScore = antichessForcedCaptureScore;
                HordeMateInOneScore = hordeMateInOneScore;
            }
        }
    }
}
