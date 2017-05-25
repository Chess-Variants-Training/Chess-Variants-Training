using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessVariantsTraining.Models
{
    public class RatingChartData
    {
        public List<string> Labels
        {
            get;
            private set;
        }

        public Dictionary<string, List<int?>> Ratings
        {
            get;
            private set;
        }

        public RatingChartData(IEnumerable<RatingWithMetadata> ratings, bool fullDateTimeOnLabel)
        {
            IEnumerable<IGrouping<DateTime, RatingWithMetadata>> groupedByDatetime = ratings.GroupBy(x => x.TimestampUtc);
            List<Element> elements = new List<Element>();
            foreach (IGrouping<DateTime, RatingWithMetadata> group in groupedByDatetime)
            {
                int? atomicRating = null;
                int? crazyhouseRating = null;
                int? kothRating = null;
                int? threeCheckRating = null;
                int? antichessRating = null;
                int? hordeRating = null;
                int? racingKingsRating = null;
                foreach (RatingWithMetadata rwm in group)
                {
                    switch (rwm.Variant)
                    {
                        case "Antichess":
                            antichessRating = (int)rwm.Rating.Value;
                            break;
                        case "Atomic":
                            atomicRating = (int)rwm.Rating.Value;
                            break;
                        case "Crazyhouse":
                            crazyhouseRating = (int)rwm.Rating.Value;
                            break;
                        case "Horde":
                            hordeRating = (int)rwm.Rating.Value;
                            break;
                        case "KingOfTheHill":
                            kothRating = (int)rwm.Rating.Value;
                            break;
                        case "RacingKings":
                            racingKingsRating = (int)rwm.Rating.Value;
                            break;
                        case "ThreeCheck":
                            threeCheckRating = (int)rwm.Rating.Value;
                            break;
                    }
                }
                DateTime timestamp = group.Key;
                string label = fullDateTimeOnLabel ? timestamp.ToString() : timestamp.ToString("D");
                elements.Add(new Element(timestamp, label, atomicRating, crazyhouseRating, kothRating, threeCheckRating, antichessRating, hordeRating, racingKingsRating));
            }

            IEnumerable<Element> ordered = elements.OrderBy(x => x.Timestamp);
            Labels = new List<string>();
            List<int?> atomicRatings = new List<int?>();
            List<int?> crazyhouseRatings = new List<int?>();
            List<int?> kothRatings = new List<int?>();
            List<int?> threeCheckRatings = new List<int?>();
            List<int?> antichessRatings = new List<int?>();
            List<int?> hordeRatings = new List<int?>();
            List<int?> racingKingsRatings = new List<int?>();
            foreach (Element elem in ordered)
            {
                Labels.Add(elem.Label);
                atomicRatings.Add(elem.AtomicRating);
                crazyhouseRatings.Add(elem.CrazyhouseRating);
                kothRatings.Add(elem.KothRating);
                threeCheckRatings.Add(elem.ThreeCheckRating);
                antichessRatings.Add(elem.AntichessRating);
                hordeRatings.Add(elem.HordeRating);
                racingKingsRatings.Add(elem.RacingKingsRating);
            }
            Ratings = new Dictionary<string, List<int?>>()
            {
                { "Antichess", antichessRatings },
                { "Atomic", atomicRatings },
                { "Crazyhouse", crazyhouseRatings },
                { "Horde", hordeRatings },
                { "King of the Hill", kothRatings },
                { "Racing Kings", racingKingsRatings },
                { "Three-check", threeCheckRatings }
            };
        }

        private class Element
        {
            public DateTime Timestamp { get; set; }
            public string Label { get; set; }
            public int? AtomicRating { get; set; }
            public int? CrazyhouseRating { get; set; }
            public int? KothRating { get; set; }
            public int? ThreeCheckRating { get; set; }
            public int? AntichessRating { get; set; }
            public int? HordeRating { get; set; }
            public int? RacingKingsRating { get; set; }

            public Element(DateTime timestamp, string label, int? atomicRating, int? crazyhouseRating, int? kothRating, int? threeCheckRating, int? antichessRating, int? hordeRating, int? racingKingsRating)
            {
                Timestamp = timestamp;
                Label = label;
                AtomicRating = atomicRating;
                CrazyhouseRating = crazyhouseRating;
                KothRating = kothRating;
                ThreeCheckRating = threeCheckRating;
                AntichessRating = antichessRating;
                HordeRating = hordeRating;
                RacingKingsRating = racingKingsRating;
            }
        }
    }
}
