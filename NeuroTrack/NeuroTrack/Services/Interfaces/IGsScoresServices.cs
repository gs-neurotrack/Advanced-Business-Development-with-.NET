using MedSave.DTOs;
using NeuroTrack.DTOs;

namespace NeuroTrack.Services.Interfaces;

public interface IGsScoresServices
{
    Task<GsScoresDTO?> GetByIdAsync(long id);

    Task<IEnumerable<GsScoresDTO>> GetAllAsync();

    Task<GsScoresDTO> AddAsync(GsScoresDTO gsScoresDto);

    Task UpdateAsync(GsScoresDTO gsScoresDto);

    Task DeleteAsync(long id);

    Task<PagedResult<GsScoresDTO>> SearchAsync(
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