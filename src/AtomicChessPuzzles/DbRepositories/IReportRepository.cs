using AtomicChessPuzzles.Models;

namespace AtomicChessPuzzles.DbRepositories
{
    public interface IReportRepository
    {
        bool Add(Report report);

        bool MarkHelpful(string reportId);

        bool MarkDeclined(string reportId);
    }
}
