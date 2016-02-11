using AtomicChessPuzzles.Models;
using MongoDB.Driver;
using System;
using System.Linq;

namespace AtomicChessPuzzles.DbRepositories
{
    public class PuzzleRepository : IPuzzleRepository
    {
        MongoSettings settings;
        IMongoCollection<Puzzle> puzzleCollection;
        Random rnd;

        public PuzzleRepository()
        {
            settings = new MongoSettings();
            rnd = new Random();
            GetCollection();
        }

        private void GetCollection()
        {
            MongoClient client = new MongoClient();
            puzzleCollection = client.GetDatabase(settings.Database).GetCollection<Puzzle>(settings.PuzzleCollectionName);
        }

        public bool Add(Puzzle puzzle)
        {
            var found = puzzleCollection.FindSync<Puzzle>(new ExpressionFilterDefinition<Puzzle>(x => x.ID == puzzle.ID));
            if (found != null && found.Any()) return false;
            try
            {
                puzzleCollection.InsertOne(puzzle);
            }
            catch (Exception e) when (e is MongoWriteException || e is MongoBulkWriteException)
            {
                return false;
            }
            return true;
        }

        public Puzzle Get(string id)
        {
            var found = puzzleCollection.FindSync<Puzzle>(new ExpressionFilterDefinition<Puzzle>(x => string.Compare(x.ID, id, true) == 0));
            if (found == null) return null;
            return found.FirstOrDefault();
        }

        public Puzzle GetOneRandomly()
        {
            ExpressionFilterDefinition<Puzzle> filter = new ExpressionFilterDefinition<Puzzle>(x => true);
            long count = puzzleCollection.Count(filter);
            if (count < 1) return null;
            return puzzleCollection.Find(filter).FirstOrDefault();
        }

        public DeleteResult Remove(string id)
        {
            return puzzleCollection.DeleteOne(x => string.Compare(x.ID, id, true) == 0);
        }

        public DeleteResult RemoveAllBy(string author)
        {
            return puzzleCollection.DeleteMany(new ExpressionFilterDefinition<Puzzle>(x => string.Compare(x.Author, author, true) == 0));
        }
    }
}
