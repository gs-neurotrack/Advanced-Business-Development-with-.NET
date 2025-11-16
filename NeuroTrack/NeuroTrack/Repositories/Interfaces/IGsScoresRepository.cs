using NeuroTrack.Models;

namespace NeuroTrack.Repositories.Interfaces;

public interface IGsScoresRepository
{
    Task<GsScores?> GetByIdAsync(long id);

    Task<IEnumerable<GsScores>> GetAllAsync();

    Task AddAsync(GsScores gsScores);

    Task UpdateAsync(GsScores gsScores);
    
    Task DeleteAsync(long id);

    Task<(IEnumerable<GsScores> Items, int TotalItems)> SearchAsync(
        long? IdScores,
        DateTime? DateScore,
        DateTime? CreatedAt,
        long? IdStatusRisk,
        long? IdUser,
        int page,
        int pageSize,
        string sortBy,
        string sortDir
    );
}