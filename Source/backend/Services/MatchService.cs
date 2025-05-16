using CoachBackend.Models;
using CoachBackend.Repositories;

namespace CoachBackend.Services;

public class MatchService
{
    private readonly MatchRepository _matchRepository;

    public MatchService(MatchRepository matchRepository)
    {
        _matchRepository = matchRepository;
    }

    public async Task<List<Match>> GetAllMatchesAsync()
    {
        return await _matchRepository.GetAllMatchesAsync();
    }

    public async Task<Match?> GetMatchByIdAsync(int id)
    {
        return await _matchRepository.GetMatchByIdAsync(id);
    }

    public async Task<Match> CreateMatchAsync(Match match)
    {
        return await _matchRepository.CreateMatchAsync(match);
    }

    public async Task<Match> UpdateMatchAsync(Match match)
    {
        return await _matchRepository.UpdateMatchAsync(match);
    }

    public async Task DeleteMatchAsync(int id)
    {
        await _matchRepository.DeleteMatchAsync(id);
    }
} 