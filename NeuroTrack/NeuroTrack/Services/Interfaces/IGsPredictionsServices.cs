using NeuroTrack.DTOs;
using NeuroTrack.DTOs.Page;

namespace NeuroTrack.Services.Interfaces;

public interface IGsPredictionsServices
{
    Task<GsPredictionsDTO?> GetByIdAsync(long id);

    Task<IEnumerable<GsPredictionsDTO>> GetAllAsync();

    Task<GsPredictionsDTO> AddAsync(GsPredictionsDTO gsPredictionsDto);

    Task UpdateAsync(GsPredictionsDTO gsPredictionsDto);

    Task DeleteAsync(long id);

    Task<PagedResult<GsPredictionsDTO>> SearchAsync(
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