using Microsoft.EntityFrameworkCore;
using NeuroTrack.Context;
using NeuroTrack.Models;
using NeuroTrack.Repositories.Interfaces;

namespace NeuroTrack.Repositories;

public class GsDailyLogRepository : IGsDailyLogRepository
{
    private readonly NeuroTrackContext _context;

    public GsDailyLogRepository(NeuroTrackContext context)
    {
        _context = context;
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) {}
    }

    public async Task<GsDailyLogs?> GetByIdAsync(long id)
    {
        var search = await _context.GsDailyLogs.FindAsync(id);

        if (search == null)
        {
            throw new NotFoundException($"User with Id {id} not found");
        }

        return search;
    }

    public async Task<IEnumerable<GsDailyLogs>> GetAllAsync()
    {
        var search = await _context.GsDailyLogs.ToListAsync();

        if (search.Count == 0)
        {
            throw new NotFoundException("Not Logs Found");
        }

        return search;
    }

    public async Task AddAsync(GsDailyLogs gsDailyLogs)
    {
        _context.GsDailyLogs.AddAsync(gsDailyLogs);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(GsDailyLogs gsDailyLogs)
    {
        var search = await _context.GsDailyLogs.FindAsync(gsDailyLogs.IdLog);

        if (search == null)
        {
            throw new NotFoundException($"Log with Id {gsDailyLogs.IdLog} not found");
        }

        _context.GsDailyLogs.Update(gsDailyLogs);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var search = await _context.GsDailyLogs.FindAsync(id);

        if (search == null)
        {
            throw new NotFoundException($"User with Id {id} not found");
        }

        _context.GsDailyLogs.Remove(search);
        await _context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<GsDailyLogs> Items, int TotalItems)> SearchAsync(
        long? IdLog, 
        int? WorkHours, 
        long? IdUser, 
        int page, 
        int pageSize, 
        string sortBy, 
        string sortDir)
    {
        var query = _context.GsDailyLogs.AsQueryable();

        if (IdLog.HasValue)
        {
            query = query.Where(g => g.IdLog == IdLog.Value);
        }

        if (WorkHours.HasValue)
        {
            query = query.Where(g => g.WorkHours == WorkHours.Value);
        }
        
        if (IdUser.HasValue)
        {
            query = query.Where(g => g.IdUser == IdUser.Value);
        }

        var totalItems = await query.CountAsync();

        bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        query = (sortBy ?? "").ToLowerInvariant() switch
        {
            "idlog" => desc ? query.OrderByDescending(g => g.IdLog) : query.OrderBy(g => g.IdLog),
            "workhours" => desc ? query.OrderByDescending(g => g.WorkHours) : query.OrderBy(g => g.WorkHours),
            "iduser" => desc ? query.OrderByDescending(g => g.IdUser) : query.OrderBy(g => g.IdUser),
            _ => desc ? query.OrderByDescending(g => g.IdLog) : query.OrderBy(g => g.IdLog)
        };

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;
        
        var skip = (page - 1) * pageSize;

        var data = await query.Skip(skip).Take(pageSize).ToListAsync();

        return (data, totalItems);
    }
}