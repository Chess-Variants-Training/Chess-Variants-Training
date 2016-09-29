using ChessVariantsTraining.Models;
using System.Collections.Generic;

namespace ChessVariantsTraining.DbRepositories
{
    public interface IReportRepository
    {
        bool Add(Report report);

        List<Report> GetUnhandledByType(string type);

        List<Report> GetUnhandledByTypes(IEnumerable<string> types);

        Report GetById(string id);

        bool Handle(string reportId, string judgement);
    }
}
