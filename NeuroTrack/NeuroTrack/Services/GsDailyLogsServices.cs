using MedSave.DTOs;
using NeuroTrack.Context;
using NeuroTrack.DTOs;
using NeuroTrack.Models;
using NeuroTrack.Repositories;
using NeuroTrack.Services.Interfaces;

namespace NeuroTrack.Services;

public class GsDailyLogsServices : IGsDailyLogsServices
{
    private readonly GsDailyLogsRepository _gsDailyLogsRepository;
    private readonly NeuroTrackContext _context;

    public GsDailyLogsServices(GsDailyLogsRepository gsDailyLogsRepository, NeuroTrackContext context)
    {
        _gsDailyLogsRepository = gsDailyLogsRepository;
        _context = context;
    }
    
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) {}
    }

    public async Task<GsDailyLogsDTO?> GetByIdAsync(long id)
    {
        var log = await _gsDailyLogsRepository.GetByIdAsync(id);

        if (log == null)
        {
            throw new NotFoundException($"Log with id {id} not found.");
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
        if (gsDailyLogsDto == null) throw new ArgumentNullException(nameof(gsDailyLogsDto), "Daily Log Object can't be null.");

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
        {
            throw new ArgumentNullException(nameof(gsDailyLogsDto), "Daily Log Object can't be null.");
        }

        var existingLog = await _gsDailyLogsRepository.GetByIdAsync(gsDailyLogsDto.IdLog);

        if (existingLog == null)
        {
            throw new NotFoundException($"Log With Id {gsDailyLogsDto.IdLog} Not Founded.");
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
        try
        {
            await _gsDailyLogsRepository.DeleteAsync(id);
        }
        catch (GsDailyLogsRepository.NotFoundException ex)
        {
            throw new GsDailyLogsRepository.NotFoundException($"Log with ID {id} not founded.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error when Trying to delete the log with id {id}: {ex.Message}");
        }
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