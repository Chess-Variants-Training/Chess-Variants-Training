using MongoDB.Bson.Serialization.Attributes;

namespace ChessVariantsTraining.Models
{
    public class Rating
    {
        [BsonElement("value")]
        public double Value { get; set; }

        [BsonElement("rd")]
        public double RatingDeviation { get; set; }

        [BsonElement("volatility")]
        public double Volatility { get; set; }

        public Rating(double value, double rd, double volatility)
        {
            Value = value;
            RatingDeviation = rd;
            Volatility = volatility;
        }
    }
}
