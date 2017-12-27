using ChessVariantsTraining.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public interface IReportRepository
    {
        bool Add(Report report);
        List<Report> GetUnhandledByType(string type);
        List<Report> GetUnhandledByTypes(IEnumerable<string> types);
        Report GetById(string id);
        bool Handle(string reportId, string judgement);

        Task<bool> AddAsync(Report report);
        Task<List<Report>> GetUnhandledByTypeAsync(string type);
        Task<List<Report>> GetUnhandledByTypesAsync(IEnumerable<string> types);
        Task<Report> GetByIdAsync(string id);
        Task<bool> HandleAsync(string reportId, string judgement);
    }
}
