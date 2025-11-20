using NeuroTrack.Models;

namespace NeuroTrack.Repositories.Interfaces;

public interface IGsPredictionsRepository
{
    Task<GsPredictions?> GetByIdAsync(long id);

    Task<IEnumerable<GsPredictions>> GetAllAsync();

    Task AddAsync(GsPredictions gsPredictions);

    Task UpdateAsync(GsPredictions gsPredictions);

    Task DeleteAsync(long id);

    Task<(IEnumerable<GsPredictions> Items, int TotalItems)> SearchAsync(
        long? IdPrediction,
        DateTime? DatePredicted,
        long? IdUser,
        long? IdScores,
        long? IdStatusRisk,
        int page,
        int pageSize,
        string sortBy,
        string sortDir
    );
}