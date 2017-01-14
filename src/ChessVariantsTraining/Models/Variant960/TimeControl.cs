using MongoDB.Bson.Serialization.Attributes;

namespace ChessVariantsTraining.Models.Variant960
{
    public class TimeControl
    {
        [BsonElement("initial")]
        public int InitialSeconds
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

        public TimeControl() { }
        public TimeControl(int secondsInitial, int secondsIncrement)
        {
            InitialSeconds = secondsInitial;
            Increment = secondsIncrement;
        }

        public override string ToString()
        {
            string minutes;
            switch (InitialSeconds)
            {
                case 30:
                    minutes = "½";
                    break;
                case 45:
                    minutes = "¾";
                    break;
                case 90:
                    minutes = "1.5";
                    break;
                default:
                    minutes = (InitialSeconds / 60).ToString();
                    break;
            }

            return string.Format("{0}+{1}", minutes, Increment);
        }
    }
}
