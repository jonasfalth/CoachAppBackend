using CoachBackend.Models;
using CoachBackend.Repositories;

namespace CoachBackend.Services;

public class PlayerService : BaseService
{
    private readonly PlayerRepository _playerRepository;
    private readonly PositionRepository _positionRepository;

    public PlayerService(PlayerRepository playerRepository, PositionRepository positionRepository)
    {
        _playerRepository = playerRepository;
        _positionRepository = positionRepository;
    }

    public async Task<List<Player>> GetAllPlayersAsync()
    {
        return await _playerRepository.GetAllPlayersAsync();
    }

    public async Task<Player?> GetPlayerByIdAsync(int id)
    {
        return await _playerRepository.GetPlayerByIdAsync(id);
    }

    public async Task<Player> CreatePlayerAsync(Player player)
    {
        ValidateModel(player);
        var position = await _positionRepository.GetPositionByIdAsync(player.PositionId);
        if (position == null)
        {
            throw new ArgumentException("Position not found");
        }
        return await _playerRepository.CreatePlayerAsync(player);
    }

    public async Task<Player> UpdatePlayerAsync(Player player)
    {
        ValidateModel(player);
        var position = await _positionRepository.GetPositionByIdAsync(player.PositionId);
        if (position == null)
        {
            throw new ArgumentException("Position not found");
        }
        return await _playerRepository.UpdatePlayerAsync(player);
    }

    public async Task DeletePlayerAsync(int id)
    {
        await _playerRepository.DeletePlayerAsync(id);
    }
} 