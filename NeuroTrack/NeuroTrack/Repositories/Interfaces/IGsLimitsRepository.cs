using NeuroTrack.Models;

namespace NeuroTrack.Repositories.Interfaces;

public interface IGsLimitsRepository
{
    Task<GsLimits?> GetByIdAsync(long id);

    Task<IEnumerable<GsLimits>> GetAllAsync();

    Task AddAsync(GsLimits gsLimits);

    Task UpdateAsync(GsLimits gsLimits);

    Task DeleteAsync(long id);

    Task<(IEnumerable<GsLimits> Items, int TotalItems)> SearchAsync(
        long? IdLimits,
        int? LimitHours,
        int? LimitMeetings,
        DateTime? CreatedAt,
        int page,
        int pageSize,
        string sortBy,
        string sortDir
    );
}