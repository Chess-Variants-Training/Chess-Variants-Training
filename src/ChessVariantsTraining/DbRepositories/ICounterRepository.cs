namespace ChessVariantsTraining.DbRepositories
{
    public interface ICounterRepository
    {
        int GetAndIncrease(string id);
    }
}
