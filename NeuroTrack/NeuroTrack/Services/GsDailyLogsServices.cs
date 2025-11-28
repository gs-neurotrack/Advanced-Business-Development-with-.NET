using NeuroTrack.DTOs;
using NeuroTrack.Context;
using NeuroTrack.DTOs.Page;
using NeuroTrack.Models;
using NeuroTrack.Repositories;
using NeuroTrack.Services.Interfaces;
using NeuroTrack.Repositories.Interfaces;

namespace NeuroTrack.Services;

public class GsDailyLogsServices : IGsDailyLogsServices
{
    private readonly IGsDailyLogsRepository _gsDailyLogsRepository;
    private readonly NeuroTrackContext _context;

    public GsDailyLogsServices(IGsDailyLogsRepository gsDailyLogsRepository, NeuroTrackContext context)
    {
        _gsDailyLogsRepository = gsDailyLogsRepository;
        _context = context;
    }

    // ❌ REMOVE ESSA INNER CLASS
    // public class NotFoundException : Exception { ... }

    public async Task<GsDailyLogsDTO?> GetByIdAsync(long id)
    {
        var log = await _gsDailyLogsRepository.GetByIdAsync(id);

        if (log == null)
        {
            // Usa a mesma NotFoundException que o controller captura
            throw new GsDailyLogsRepository.NotFoundException($"Log with id {id} not found.");
        }

        return new GsDailyLogsDTO
        (
            IdLog : log.IdLog,
            IdUser : log.IdUser,
            LogDate : log.LogDate,
            Meetings : log.Meetings,
            WorkHours : log.WorkHours
        );
    }

    public async Task<IEnumerable<GsDailyLogsDTO>> GetAllAsync()
    {
        var logs = await _gsDailyLogsRepository.GetAllAsync();

        return logs.Select(log => new GsDailyLogsDTO(
            IdLog : log.IdLog,
            IdUser : log.IdUser,
            LogDate : log.LogDate,
            Meetings : log.Meetings,
            WorkHours : log.WorkHours
        )).ToList();
    }

    public async Task<GsDailyLogsDTO> AddAsync(GsDailyLogsDTO gsDailyLogsDto)
    {
        if (gsDailyLogsDto == null)
            throw new ArgumentNullException(nameof(gsDailyLogsDto), "Daily Log Object can't be null.");

        var log = new GsDailyLogs
        {
            IdLog = gsDailyLogsDto.IdLog,
            IdUser = gsDailyLogsDto.IdUser,
            LogDate = gsDailyLogsDto.LogDate,
            Meetings = gsDailyLogsDto.Meetings,
            WorkHours = gsDailyLogsDto.WorkHours
        };

        await _gsDailyLogsRepository.AddAsync(log);

        return new GsDailyLogsDTO(
            IdLog: log.IdLog,
            IdUser: log.IdUser,
            LogDate: log.LogDate,
            Meetings: log.Meetings,
            WorkHours: log.WorkHours
        );
    }

    public async Task UpdateAsync(GsDailyLogsDTO gsDailyLogsDto)
    {
        if (gsDailyLogsDto == null)
            throw new ArgumentNullException(nameof(gsDailyLogsDto), "Daily Log Object can't be null.");

        var existingLog = await _gsDailyLogsRepository.GetByIdAsync(gsDailyLogsDto.IdLog);

        if (existingLog == null)
        {
            // Mesma exceção do repositório
            throw new GsDailyLogsRepository.NotFoundException($"Log with Id {gsDailyLogsDto.IdLog} not found.");
        }

        existingLog.IdLog = gsDailyLogsDto.IdLog;
        existingLog.IdUser = gsDailyLogsDto.IdUser;
        existingLog.LogDate = gsDailyLogsDto.LogDate;
        existingLog.Meetings = gsDailyLogsDto.Meetings;
        existingLog.WorkHours = gsDailyLogsDto.WorkHours;

        await _gsDailyLogsRepository.UpdateAsync(existingLog);
    }

    public async Task DeleteAsync(long id)
    {
        // Não precisa capturar e relançar, deixa a NotFoundException subir
        await _gsDailyLogsRepository.DeleteAsync(id);
    }

    public async Task<PagedResult<GsDailyLogsDTO>> SearchAsync(
        long? IdLog, 
        int? WorkHours, 
        long? IdUser, 
        int page, 
        int pageSize, 
        string sortBy, 
        string sortDir)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;
        sortBy ??= "idLog";
        sortDir ??= "asc";

        var (items, total) = await _gsDailyLogsRepository.SearchAsync(
            IdLog, WorkHours, IdUser, page, pageSize, sortBy, sortDir
        );

        var dtoItems = items.Select(log => new GsDailyLogsDTO(
            IdLog: log.IdLog,
            WorkHours: log.WorkHours,
            Meetings: log.Meetings,
            LogDate: log.LogDate,
            IdUser: log.IdUser
        )).ToList();
        
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);

        return new PagedResult<GsDailyLogsDTO>
        {
            Items = dtoItems,
            PageInfo = new PageInfo
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                TotalPages = totalPages
            }
        };
    }
}
