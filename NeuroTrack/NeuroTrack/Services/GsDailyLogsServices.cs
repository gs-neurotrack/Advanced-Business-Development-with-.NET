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
        if (gsDailyLogsDto == null) throw new ArgumentNullException(nameof(gsDailyLogsDto));

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
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(long id)
    {
        throw new NotImplementedException();
    }
}