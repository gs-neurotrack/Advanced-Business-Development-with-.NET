using MedSave.DTOs;
using NeuroTrack.DTOs;

namespace NeuroTrack.Services.Interfaces;

public interface IGsDailyLogServices
{
    Task<GsDailyLogsDTO?> GetByIdAsync(long id);

    Task<IEnumerable<GsDailyLogsDTO>> GetAllAsync();

    Task<GsDailyLogsDTO> AddAsync(GsDailyLogsDTO gsDailyLogsDto);

    Task UpdateAsync(GsDailyLogsDTO gsDailyLogsDto);

    Task DeleteAsync(long id);

    Task<PagedResult<GsDailyLogsDTO>> SearchAsync(
        long? IdLog,
        int? WorkHours,
        long? IdUser,
        int page,
        int pageSize,
        string sortBy,
        string sortDir
    );
}