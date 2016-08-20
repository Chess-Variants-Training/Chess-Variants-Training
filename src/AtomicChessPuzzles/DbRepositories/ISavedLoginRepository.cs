using AtomicChessPuzzles.Models;

namespace AtomicChessPuzzles.DbRepositories
{
    public interface ISavedLoginRepository
    {
        void Add(SavedLogin login);

        bool ContainsID(long id);

        int? AuthenticatedUser(long loginId, byte[] hashedToken);

        void Delete(long id);
    }
}
