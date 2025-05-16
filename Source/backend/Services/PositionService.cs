using CoachBackend.Models;
using CoachBackend.Repositories;

namespace CoachBackend.Services;

public class PositionService
{
    private readonly PositionRepository _positionRepository;

    public PositionService(PositionRepository positionRepository)
    {
        _positionRepository = positionRepository;
    }

    public async Task<List<Position>> GetAllPositionsAsync()
    {
        return await _positionRepository.GetAllPositionsAsync();
    }

    public async Task<Position?> GetPositionByIdAsync(int id)
    {
        return await _positionRepository.GetPositionByIdAsync(id);
    }

    public async Task<Position> CreatePositionAsync(Position position)
    {
        return await _positionRepository.CreatePositionAsync(position);
    }

    public async Task<Position> UpdatePositionAsync(Position position)
    {
        return await _positionRepository.UpdatePositionAsync(position);
    }

    public async Task DeletePositionAsync(int id)
    {
        await _positionRepository.DeletePositionAsync(id);
    }
} 