using AtomicChessPuzzles.Models;
using System.Collections.Generic;

namespace AtomicChessPuzzles.DbRepositories
{
    public interface IReportRepository
    {
        bool Add(Report report);

        bool MarkHelpful(string reportId);

        bool MarkDeclined(string reportId);

        List<Report> GetByType(string type);

        List<Report> GetByTypes(IEnumerable<string> types);
    }
}
