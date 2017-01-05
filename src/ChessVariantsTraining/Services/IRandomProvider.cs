namespace ChessVariantsTraining.Services
{
    public interface IRandomProvider
    {
        bool RandomBool();

        string RandomString(int length);

        int RandomPositiveInt(int maxExclusive);
    }
}
