using Microsoft.EntityFrameworkCore;
using NeuroTrack.Context;
using NeuroTrack.Models;
using NeuroTrack.Repositories.Interfaces;

namespace NeuroTrack.Repositories;

public class GsLimitsRepository : IGsLimitsRepository
{
    private readonly NeuroTrackContext _context;

    public GsLimitsRepository(NeuroTrackContext context)
    {
        _context = context;
    }
    
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) {}
    }

    public async Task<GsLimits?> GetByIdAsync(long id)
    {
        return await _context.GsLimits.FindAsync(id);
    }

    public async Task<IEnumerable<GsLimits>> GetAllAsync()
    {
        return await _context.GsLimits.ToListAsync();
    }

    public async Task AddAsync(GsLimits gsLimits)
    {
        _context.GsLimits.AddAsync(gsLimits);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(GsLimits gsLimits)
    {
        _context.GsLimits.Update(gsLimits);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var search = await _context.GsLimits.FindAsync(id);

        if (search == null)
        {
            throw new NotFoundException($"Limit with Id {id} not found");
        }

        _context.GsLimits.Remove(search);
        await _context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<GsLimits> Items, int TotalItems)> SearchAsync(
        long? IdLimits, 
        int? LimitHours, 
        int? LimitMeetings, 
        DateTime? CreatedAt, 
        int page, 
        int pageSize, 
        string sortBy, 
        string sortDir)
    {
        var query = _context.GsLimits.AsQueryable();

        if (IdLimits.HasValue)
        {
            query = query.Where(g => g.IdLimits == IdLimits.Value);
        }

        if (LimitHours.HasValue)
        {
            query = query.Where(g => g.LimitHours == LimitHours.Value);
        }

        if (LimitMeetings.HasValue)
        {
            query = query.Where(g => g.LimitMeetings == LimitMeetings.Value);
        }

        if (CreatedAt.HasValue)
        {
            query = query.Where(g => g.CreatedAt == CreatedAt.Value);
        }

        var totalItems = await query.CountAsync();
        
        bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        
        query = (sortBy ?? "").ToLowerInvariant() switch
        {
            "idlimits" => desc ? query.OrderByDescending(g => g.IdLimits) : query.OrderBy(g => g.IdLimits),
            "limithours" => desc ? query.OrderByDescending(g => g.LimitHours) : query.OrderBy(g => g.LimitHours),
            "limitmeetings" => desc ? query.OrderByDescending(g => g.LimitMeetings) : query.OrderBy(g => g.LimitMeetings),
            "createdat" => desc ? query.OrderByDescending(g => g.CreatedAt) : query.OrderBy(g => g.CreatedAt),
            _ => desc ? query.OrderByDescending(g => g.IdLimits) : query.OrderBy(g => g.IdLimits)
        };
        
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;
        
        var skip = (page - 1) * pageSize;

        var data = await query.Skip(skip).Take(pageSize).ToListAsync();

        return (data, totalItems);

    }
}