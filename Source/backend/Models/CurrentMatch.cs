using System.ComponentModel.DataAnnotations;

namespace CoachBackend.Models;

public class CurrentMatch
{
    public int Id { get; set; }
    
    [Required]
    public int MatchId { get; set; }
    
    [Required]
    public string Status { get; set; } = "setup"; // setup, first_half, half_time, second_half, finished, paused
    
    public DateTime? MatchStartTime { get; set; }
    public DateTime? FirstHalfStartTime { get; set; }
    public DateTime? SecondHalfStartTime { get; set; }
    public DateTime? LastPauseTime { get; set; }
    
    public int FirstHalfDurationSeconds { get; set; } = 0;
    public int SecondHalfDurationSeconds { get; set; } = 0;
    public int TotalPauseSeconds { get; set; } = 0;
    
    public int HomeScore { get; set; } = 0;
    public int AwayScore { get; set; } = 0;
    
    public string? Formation { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
} 