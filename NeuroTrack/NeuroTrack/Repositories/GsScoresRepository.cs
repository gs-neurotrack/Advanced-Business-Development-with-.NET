using Microsoft.EntityFrameworkCore;
using NeuroTrack.Context;
using NeuroTrack.Models;
using NeuroTrack.Repositories.Interfaces;

namespace NeuroTrack.Repositories;

public class GsScoresRepository :IGsScoresRepository
{
    private readonly NeuroTrackContext _context;

    public GsScoresRepository(NeuroTrackContext context)
    {
        _context = context;
    }
    
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) {}
    }

    public async Task<GsScores?> GetByIdAsync(long id)
    {
        var search = await _context.GsScores.FindAsync(id);
        
        if (search == null)
        {
            throw new NotFoundException($"Score with Id {id} not found");
        }

        return search;
    }
    
    public async Task<IEnumerable<GsScores>> GetAllAsync()
    {
        var search = await _context.GsScores.ToListAsync();

        if (search.Count == 0)
        {
            throw new NotFoundException("Not Scores Found");
        }

        return search;
    }
    
    public async Task AddAsync(GsScores gsScores)
    {
        _context.GsScores.AddAsync(gsScores);
        await _context.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(GsScores gsScores)
    {
        var search = await _context.GsScores.FindAsync(gsScores.IdScores);

        if (search == null)
        {
            throw new NotFoundException($"Limit with Id {gsScores.IdScores} not found");
        }

        _context.GsScores.Update(gsScores);
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(long id)
    {
        var search = await _context.GsScores.FindAsync(id);

        if (search == null)
        {
            throw new NotFoundException($"Score with Id {id} not found");
        }

        _context.GsScores.Remove(search);
        await _context.SaveChangesAsync();
    }


    public async Task<(IEnumerable<GsScores> Items, int TotalItems)> SearchAsync(
        long? IdScores, 
        DateTime? DateScore, 
        DateTime? CreatedAt, 
        long? IdStatusRisk, 
        long? IdUser, 
        int page, 
        int pageSize, 
        string sortBy, 
        string sortDir)
    {
        var query = _context.GsScores.AsQueryable();
        
        if (IdScores.HasValue)
        {
            query = query.Where(g => g.IdScores == IdScores.Value);
        }
        
        if (DateScore.HasValue)
        {
            query = query.Where(g => g.DateScore == DateScore.Value);
        }
        
        if (CreatedAt.HasValue)
        {
            query = query.Where(g => g.CreatedAt == CreatedAt.Value);
        }
        
        if (IdStatusRisk.HasValue)
        {
            query = query.Where(g => g.IdStatusRisk == IdStatusRisk.Value);
        }
        
        if (IdUser.HasValue)
        {
            query = query.Where(g => g.IdUser == IdUser.Value);
        }

        var totalItems = await query.CountAsync();
        
        bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

        query = (sortBy ?? "").ToLowerInvariant() switch
        {
            "idscores" => desc ? query.OrderByDescending(g => g.IdScores) : query.OrderBy(g => g.IdScores),
            "datescore" => desc ? query.OrderByDescending(g => g.DateScore) : query.OrderBy(g => g.DateScore),
            "createdat" => desc ? query.OrderByDescending(g => g.CreatedAt) : query.OrderBy(g => g.CreatedAt),
            "idstatusrisk" => desc ? query.OrderByDescending(g => g.IdStatusRisk) : query.OrderBy(g => g.IdStatusRisk),
            "iduser" => desc ? query.OrderByDescending(g => g.IdUser) : query.OrderBy(g => g.IdUser),
            _ => desc ? query.OrderByDescending(g => g.IdScores) : query.OrderBy(g => g.IdScores)
        };
        
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;
        
        var skip = (page - 1) * pageSize;

        var data = await query.Skip(skip).Take(pageSize).ToListAsync();

        return (data, totalItems);

    }
}