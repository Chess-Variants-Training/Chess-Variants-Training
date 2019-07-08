using ChessVariantsTraining.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public interface ITagRepository
    {
        Task<List<PuzzleTag>> TagsByVariantAsync(string variant);
        Task MaybeAddTagAsync(string variant, string tag);
        Task MaybeRemoveTagAsync(string variant, string tag);
    }
}
