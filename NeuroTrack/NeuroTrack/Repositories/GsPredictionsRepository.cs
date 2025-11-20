using Microsoft.EntityFrameworkCore;
using NeuroTrack.Context;
using NeuroTrack.Models;
using NeuroTrack.Repositories.Interfaces;

namespace NeuroTrack.Repositories;

public class GsPredictionsRepository : IGsPredictionsRepository
{
    private readonly NeuroTrackContext _context;

    public GsPredictionsRepository(NeuroTrackContext context)
    {
        _context = context;
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) {}
    }

    public async Task<GsPredictions?> GetByIdAsync(long id)
    {
        return await _context.GsPredictions.FindAsync(id);
    }

    public async Task<IEnumerable<GsPredictions>> GetAllAsync()
    {
        return await _context.GsPredictions.ToListAsync();
    }

    public async Task AddAsync(GsPredictions gsPredictions)
    {
        _context.GsPredictions.AddAsync(gsPredictions);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(GsPredictions gsPredictions)
    {
        _context.GsPredictions.Update(gsPredictions);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var search = await _context.GsPredictions.FindAsync(id);

        if (search == null)
        {
            throw new NotFoundException($"Prediction with Id {id} not found");
        }

        _context.GsPredictions.Remove(search);
        await _context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<GsPredictions> Items, int TotalItems)> SearchAsync(
        long? IdPrediction, 
        DateTime? DatePredicted, 
        long? IdUser, 
        long? IdScores, 
        long? IdStatusRisk, 
        int page, 
        int pageSize, 
        string sortBy, 
        string sortDir)
    {
        var query = _context.GsPredictions.AsQueryable();

        if (IdPrediction.HasValue)
        {
            query = query.Where(g => g.IdPrediction == IdPrediction.Value);
        }

        if (DatePredicted.HasValue)
        {
            query = query.Where(g => g.DatePredicted == DatePredicted.Value);
        }

        if (IdUser.HasValue)
        {
            query = query.Where(g => g.IdUser == IdUser.Value);
        }

        if (IdScores.HasValue)
        {
            query = query.Where(g => g.IdScores == IdScores.Value);
        }

        if (IdStatusRisk.HasValue)
        {
            query = query.Where(g => g.IdStatusRisk == IdStatusRisk.Value);
        }

        var totalItems = await query.CountAsync();
        
        bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        query = (sortBy ?? "").ToLowerInvariant() switch
        {
            "idPrediction" => desc ? query.OrderByDescending(g => g.IdPrediction) : query.OrderBy(g => g.IdPrediction),
            "datePredicted" => desc ? query.OrderByDescending(g => g.DatePredicted) : query.OrderBy(g => g.DatePredicted),
            "idUser" => desc ? query.OrderByDescending(g => g.IdUser) : query.OrderBy(g => g.IdUser),
            "idScores" => desc ? query.OrderByDescending(g => g.IdScores) : query.OrderBy(g => g.IdScores),
            "idStatusRisk" => desc ? query.OrderByDescending(g => g.IdStatusRisk) : query.OrderBy(g => g.IdStatusRisk),
            _ => desc ? query.OrderByDescending(g => g.IdPrediction) : query.OrderBy(g => g.IdPrediction),
        };
        
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;
        
        var skip = (page - 1) * pageSize;

        var data = await query.Skip(skip).Take(pageSize).ToListAsync();

        return (data, totalItems);
    }
}