using NeuroTrack.Context;
using NeuroTrack.DTOs;
using NeuroTrack.DTOs.Page;
using NeuroTrack.Models;
using NeuroTrack.Repositories.Interfaces;
using NeuroTrack.Services.Interfaces;
using NeuroTrack.Repositories;

namespace NeuroTrack.Services;

public class GsPredictionsServices : IGsPredictionsServices
{
    private readonly IGsPredictionsRepository _gsPredictionsRepository;
    private readonly NeuroTrackContext _context;

    public GsPredictionsServices(IGsPredictionsRepository gsPredictionsRepository, NeuroTrackContext context)
    {
        _gsPredictionsRepository = gsPredictionsRepository;
        _context = context;
    }
    
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) {}
    }

    public async Task<GsPredictionsDTO?> GetByIdAsync(long id)
    {
        var prediction = await _gsPredictionsRepository.GetByIdAsync(id);

        if (prediction == null)
        {
            throw new NotFoundException($"Prediction with ID {id} not found.");
        }

        return new GsPredictionsDTO(
            IdPrediction: prediction.IdPrediction,
            StressPredicted: prediction.StressPredicted,
            Message: prediction.Message,
            DatePredicted: prediction.DatePredicted,
            IdUser: prediction.IdUser,
            IdScores: prediction.IdScores,
            IdStatusRisk: prediction.IdStatusRisk
        );
    }

    public async Task<IEnumerable<GsPredictionsDTO>> GetAllAsync()
    {
        var predictions = await _gsPredictionsRepository.GetAllAsync();

        return predictions.Select(prediction => new GsPredictionsDTO(
            IdPrediction: prediction.IdPrediction,
            StressPredicted: prediction.StressPredicted,
            Message: prediction.Message,
            DatePredicted: prediction.DatePredicted,
            IdUser: prediction.IdUser,
            IdScores: prediction.IdScores,
            IdStatusRisk: prediction.IdStatusRisk
        )).ToList();
    }

    public async Task<GsPredictionsDTO> AddAsync(GsPredictionsDTO gsPredictionsDto)
    {
        if (gsPredictionsDto == null) throw new ArgumentNullException(nameof(gsPredictionsDto), "Prediction Object can't be null.");

        var prediction = new GsPredictions
        {
            IdPrediction = gsPredictionsDto.IdPrediction,
            StressPredicted = gsPredictionsDto.StressPredicted,
            Message = gsPredictionsDto.Message,
            DatePredicted = gsPredictionsDto.DatePredicted,
            IdUser = gsPredictionsDto.IdUser,
            IdScores = gsPredictionsDto.IdScores,
            IdStatusRisk = gsPredictionsDto.IdStatusRisk
        };

        await _gsPredictionsRepository.AddAsync(prediction);

        return new GsPredictionsDTO(
            IdPrediction: prediction.IdPrediction,
            StressPredicted: prediction.StressPredicted,
            Message: prediction.Message,
            DatePredicted: prediction.DatePredicted,
            IdUser: prediction.IdUser,
            IdScores: prediction.IdScores,
            IdStatusRisk: prediction.IdStatusRisk
        );
    }

    public async Task UpdateAsync(GsPredictionsDTO gsPredictionsDto)
    {
        if (gsPredictionsDto == null) throw new ArgumentNullException(nameof(gsPredictionsDto), "Prediction Object can't be null.");

        var existingPrediction = await _gsPredictionsRepository.GetByIdAsync(gsPredictionsDto.IdPrediction);

        if (existingPrediction == null)
        {
            throw new NotFoundException($"Prediction with id {gsPredictionsDto.IdPrediction} not founded.");
        }

        existingPrediction.IdPrediction = gsPredictionsDto.IdPrediction;
        existingPrediction.StressPredicted = gsPredictionsDto.StressPredicted;
        existingPrediction.Message = gsPredictionsDto.Message;
        existingPrediction.DatePredicted = gsPredictionsDto.DatePredicted;
        existingPrediction.IdUser = gsPredictionsDto.IdUser;
        existingPrediction.IdScores = gsPredictionsDto.IdScores;
        existingPrediction.IdStatusRisk = gsPredictionsDto.IdStatusRisk;

        await _gsPredictionsRepository.UpdateAsync(existingPrediction);
    }

    public async Task DeleteAsync(long id)
    {
        try
        {
            await _gsPredictionsRepository.DeleteAsync(id);
        }
        catch (GsPredictionsRepository.NotFoundException ex)
        {
            throw new GsPredictionsRepository.NotFoundException($"Prediction with id {id} not founded.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error when trying to delete the prediction with id {id}: {ex.Message}");
        }
    }

    public async Task<PagedResult<GsPredictionsDTO>> SearchAsync(
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
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;
        sortBy ??= "idPrediction";
        sortDir ??= "asc";

        var (items, total) = await _gsPredictionsRepository.SearchAsync(IdPrediction, DatePredicted, IdUser, IdScores, IdStatusRisk, page, pageSize, sortBy, sortDir);

        var dtoItems = items.Select(prediction => new GsPredictionsDTO(
            IdPrediction: prediction.IdPrediction,
            StressPredicted: prediction.StressPredicted,
            Message: prediction.Message,
            DatePredicted: prediction.DatePredicted,
            IdUser: prediction.IdUser,
            IdScores: prediction.IdScores,
            IdStatusRisk: prediction.IdStatusRisk
        )).ToList();
        
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);

        return new PagedResult<GsPredictionsDTO>
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