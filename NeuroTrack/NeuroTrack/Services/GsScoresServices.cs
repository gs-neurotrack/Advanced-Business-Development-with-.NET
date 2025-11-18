using NeuroTrack.DTOs;
using NeuroTrack.Context;
using NeuroTrack.DTOs.Page;
using NeuroTrack.Models;
using NeuroTrack.Repositories;
using NeuroTrack.Services.Interfaces;

namespace NeuroTrack.Services;

public class GsScoresServices : IGsScoresServices
{
    private readonly GsScoresRepository _gsScoresRepository;
    private readonly NeuroTrackContext _context;

    public GsScoresServices(GsScoresRepository gsScoresRepository, NeuroTrackContext context)
    {
        _gsScoresRepository = gsScoresRepository;
        _context = context;
    }
    
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) {}
    }

    public async Task<GsScoresDTO?> GetByIdAsync(long id)
    {
        var score = await _gsScoresRepository.GetByIdAsync(id);

        if (score == null)
        {
            throw new NotFoundException($"Score with id {id} not found.");
        }

        return new GsScoresDTO(
            IdScores: score.IdScores,
            DateScore: score.DateScore,
            ScoreValue: score.ScoreValue,
            TimeRecommendation: score.TimeRecommendation,
            CreatedAt: score.CreatedAt,
            IdStatusRisk: score.IdStatusRisk,
            IdUser: score.IdUser,
            IdLog: score.IdLog
        );
    }

    public async Task<IEnumerable<GsScoresDTO>> GetAllAsync()
    {
        var scores = await _gsScoresRepository.GetAllAsync();

        return scores.Select(score => new GsScoresDTO(
            IdScores: score.IdScores,
            DateScore: score.DateScore,
            ScoreValue: score.ScoreValue,
            TimeRecommendation: score.TimeRecommendation,
            CreatedAt: score.CreatedAt,
            IdStatusRisk: score.IdStatusRisk,
            IdUser: score.IdUser,
            IdLog: score.IdLog
        )).ToList();
    }

    public async Task<GsScoresDTO> AddAsync(GsScoresDTO gsScoresDto)
    {
        if (gsScoresDto == null) throw new ArgumentNullException(nameof(gsScoresDto), "Score Object can't be null.");

        var score = new GsScores
        {
            CreatedAt = gsScoresDto.CreatedAt,
            DateScore = gsScoresDto.DateScore,
            IdLog = gsScoresDto.IdLog,
            IdScores = gsScoresDto.IdScores,
            IdStatusRisk = gsScoresDto.IdStatusRisk,
            ScoreValue = gsScoresDto.ScoreValue,
            IdUser = gsScoresDto.IdUser,
            TimeRecommendation = gsScoresDto.TimeRecommendation
        };

        await _gsScoresRepository.AddAsync(score);

        return new GsScoresDTO(
            IdScores: score.IdScores,
            DateScore: score.DateScore,
            ScoreValue: score.ScoreValue,
            TimeRecommendation: score.TimeRecommendation,
            CreatedAt: score.CreatedAt,
            IdStatusRisk: score.IdStatusRisk,
            IdUser: score.IdUser,
            IdLog: score.IdLog
        );
    }

    public async Task UpdateAsync(GsScoresDTO gsScoresDto)
    {
        if (gsScoresDto == null) throw new ArgumentNullException(nameof(gsScoresDto), "Score Object can't be null.");

        var existingScore = await _gsScoresRepository.GetByIdAsync(gsScoresDto.IdScores);
        
        if (existingScore == null)
        {
            throw new NotFoundException($"Score With Id {gsScoresDto.IdScores} Not Founded.");
        }

        existingScore.IdScores = gsScoresDto.IdScores;
        existingScore.CreatedAt = gsScoresDto.CreatedAt;
        existingScore.DateScore = gsScoresDto.DateScore;
        existingScore.IdLog = gsScoresDto.IdLog;
        existingScore.IdStatusRisk = gsScoresDto.IdStatusRisk;
        existingScore.IdUser = gsScoresDto.IdUser;
        existingScore.ScoreValue = gsScoresDto.ScoreValue;
        existingScore.TimeRecommendation = gsScoresDto.TimeRecommendation;

        await _gsScoresRepository.UpdateAsync(existingScore);
    }

    public async Task DeleteAsync(long id)
    {
        try
        {
            await _gsScoresRepository.DeleteAsync(id);
        }
        catch (GsScoresRepository.NotFoundException ex)
        {
            throw new GsScoresRepository.NotFoundException($"Score with Id {id} not founded.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error when trying to delete the log with Id {id}: {ex.Message}");
        }
    }

    public async Task<PagedResult<GsScoresDTO>> SearchAsync(
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
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;
        sortBy ??= "idScores";
        sortDir ??= "asc";

        var (items, total) = await _gsScoresRepository.SearchAsync(
            IdScores, DateScore, CreatedAt, IdStatusRisk, IdUser, page, pageSize, sortBy, sortDir
        );

        var dtoItems = items.Select(score => new GsScoresDTO(
            IdScores: score.IdScores,
            DateScore: score.DateScore,
            ScoreValue: score.ScoreValue,
            TimeRecommendation: score.TimeRecommendation,
            CreatedAt: score.CreatedAt,
            IdStatusRisk: score.IdStatusRisk,
            IdUser: score.IdUser,
            IdLog: score.IdLog
        )).ToList();
        
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);

        return new PagedResult<GsScoresDTO>
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