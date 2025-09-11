using CoachBackend.Models;
using CoachBackend.Repositories;

namespace CoachBackend.Services;

public class CurrentMatchService : BaseService
{
    private readonly CurrentMatchRepository _currentMatchRepository;
    private readonly MatchRepository _matchRepository;

    public CurrentMatchService(CurrentMatchRepository currentMatchRepository, MatchRepository matchRepository)
    {
        _currentMatchRepository = currentMatchRepository;
        _matchRepository = matchRepository;
    }

    public async Task<CurrentMatch?> GetActiveCurrentMatchAsync()
    {
        return await _currentMatchRepository.GetActiveCurrentMatchAsync();
    }

    public async Task<CurrentMatch?> GetCurrentMatchByIdAsync(int id)
    {
        return await _currentMatchRepository.GetCurrentMatchByIdAsync(id);
    }

    public async Task<CurrentMatch> CreateCurrentMatchAsync(CurrentMatch currentMatch)
    {
        ValidateModel(currentMatch);
        
        // Validate that match exists
        var match = await _matchRepository.GetMatchByIdAsync(currentMatch.MatchId);
        if (match == null)
        {
            throw new ArgumentException("Match not found");
        }

        return await _currentMatchRepository.CreateCurrentMatchAsync(currentMatch);
    }

    public async Task<CurrentMatch> UpdateCurrentMatchAsync(CurrentMatch currentMatch)
    {
        ValidateModel(currentMatch);
        
        // Validate that match exists
        var match = await _matchRepository.GetMatchByIdAsync(currentMatch.MatchId);
        if (match == null)
        {
            throw new ArgumentException("Match not found");
        }

        return await _currentMatchRepository.UpdateCurrentMatchAsync(currentMatch);
    }
} 