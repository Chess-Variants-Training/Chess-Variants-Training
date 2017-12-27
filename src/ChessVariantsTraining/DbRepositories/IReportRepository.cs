using ChessVariantsTraining.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChessVariantsTraining.DbRepositories
{
    public interface IReportRepository
    {
        Task<bool> AddAsync(Report report);
        Task<List<Report>> GetUnhandledByTypeAsync(string type);
        Task<List<Report>> GetUnhandledByTypesAsync(IEnumerable<string> types);
        Task<Report> GetByIdAsync(string id);
        Task<bool> HandleAsync(string reportId, string judgement);
    }
}
