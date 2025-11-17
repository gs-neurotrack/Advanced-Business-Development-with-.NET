using MedSave.DTOs;
using NeuroTrack.DTOs;

namespace NeuroTrack.Services.Interfaces;

public interface IGsLimitsServices
{
    Task<GsLimitsDTO?> GetByIdAsync(long id);

    Task<IEnumerable<GsLimitsDTO>> GetAllAsync();

    Task<GsLimitsDTO> AddAsync(GsLimitsDTO gsLimitsDto);

    Task UpdateAsync(GsLimitsDTO gsLimitsDto);

    Task DeleteAsync(long id);

    Task<PagedResult<GsLimitsDTO>> SearchAsync(
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