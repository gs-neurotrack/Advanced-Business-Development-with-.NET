using MedSave.DTOs;
using NeuroTrack.Context;
using NeuroTrack.DTOs;
using NeuroTrack.Models;
using NeuroTrack.Repositories;
using NeuroTrack.Services.Interfaces;

namespace NeuroTrack.Services;

public class GsLimitsServices : IGsLimitsServices
{
    private readonly GsLimitsRepository _gsLimitsRepository;
    private readonly NeuroTrackContext _context;

    public GsLimitsServices(GsLimitsRepository gsLimitsRepository, NeuroTrackContext context)
    {
        _gsLimitsRepository = gsLimitsRepository;
        _context = context;
    }
    
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) {}
    }

    public async Task<GsLimitsDTO?> GetByIdAsync(long id)
    {
        var limit = await _gsLimitsRepository.GetByIdAsync(id);
        
        if (limit == null)
        {
            throw new NotFoundException($"Log with id {id} not found.");
        }

        return new GsLimitsDTO(
            IdLimits: limit.IdLimits,
            LimitHours: limit.LimitHours,
            LimitMeetings: limit.LimitMeetings,
            CreatedAt: limit.CreatedAt
        );

    }

    public async Task<IEnumerable<GsLimitsDTO>> GetAllAsync()
    {
        var limits = await _gsLimitsRepository.GetAllAsync();

        return limits.Select(limit => new GsLimitsDTO(
            IdLimits: limit.IdLimits,
            LimitHours: limit.LimitHours,
            LimitMeetings: limit.LimitMeetings,
            CreatedAt: limit.CreatedAt
        )).ToList();
    }

    public async Task<GsLimitsDTO> AddAsync(GsLimitsDTO gsLimitsDto)
    {
        if (gsLimitsDto == null) throw new ArgumentNullException(nameof(gsLimitsDto), "Limit Object can't be null.");

        var limit = new GsLimits
        {
            CreatedAt = gsLimitsDto.CreatedAt,
            IdLimits = gsLimitsDto.IdLimits,
            LimitHours = gsLimitsDto.LimitHours,
            LimitMeetings = gsLimitsDto.LimitMeetings
        };

        await _gsLimitsRepository.AddAsync(limit);

        return new GsLimitsDTO(
            IdLimits: limit.IdLimits,
            LimitHours: limit.LimitHours,
            LimitMeetings: limit.LimitMeetings,
            CreatedAt: limit.CreatedAt
        );
    }

    public async Task UpdateAsync(GsLimitsDTO gsLimitsDto)
    {
        if (gsLimitsDto == null) throw new ArgumentNullException(nameof(gsLimitsDto), "Limit Object can't be null.");

        var existingLimit = await _gsLimitsRepository.GetByIdAsync(gsLimitsDto.IdLimits);
        
        if (existingLimit == null)
        {
            throw new NotFoundException($"Limit With Id {gsLimitsDto.IdLimits} Not Founded.");
        }

        existingLimit.IdLimits = gsLimitsDto.IdLimits;
        existingLimit.CreatedAt = gsLimitsDto.CreatedAt;
        existingLimit.LimitHours = gsLimitsDto.LimitHours;
        existingLimit.LimitMeetings = gsLimitsDto.LimitMeetings;

        await _gsLimitsRepository.UpdateAsync(existingLimit);
    }

    public async Task DeleteAsync(long id)
    {
        try
        {
            await _gsLimitsRepository.DeleteAsync(id);
        }
        catch (GsLimitsRepository.NotFoundException ex)
        {
            throw new GsLimitsRepository.NotFoundException($"Limit with Id {id} not founded.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error when trying to delete the log with id {id}: {ex.Message}");
        }
    }

    public async Task<PagedResult<GsLimitsDTO>> SearchAsync(
        long? IdLimits, 
        int? LimitHours, 
        int? LimitMeetings, 
        DateTime? CreatedAt, 
        int page, 
        int pageSize, 
        string sortBy, 
        string sortDir)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;
        sortBy ??= "idLimits";
        sortDir ??= "asc";

        var (items, total) = await _gsLimitsRepository.SearchAsync(
            IdLimits, LimitHours, LimitMeetings, CreatedAt, page, pageSize, sortBy, sortDir
        );

        var dtoItems = items.Select(limit => new GsLimitsDTO(
            IdLimits: limit.IdLimits,
            LimitHours: limit.LimitHours,
            LimitMeetings: limit.LimitMeetings,
            CreatedAt: limit.CreatedAt
        )).ToList();
        
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);

        return new PagedResult<GsLimitsDTO>
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