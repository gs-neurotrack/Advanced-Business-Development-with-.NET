using NeuroTrack.Models;

namespace NeuroTrack.Repositories.Interfaces;

public interface IGsDailyLogRepository
{
    Task<GsDailyLogs?> GetByIdAsync(long id);

    Task<IEnumerable<GsDailyLogs>> GetAllAsync();

    Task AddAsync (GsDailyLogs gsDailyLogs);

    Task UpdateAsync (GsDailyLogs gsDailyLogs);

    Task DeleteAsync (long id);

    Task<(IEnumerable<GsDailyLogs> Items, int TotalItems)> SearchAsync(
        long? IdLog,
        int? WorkHours,
        long? IdUser,
        int page,
        int pageSize,
        string sortBy,
        string sortDir
    );
}