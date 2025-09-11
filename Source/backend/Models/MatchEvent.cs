using System.ComponentModel.DataAnnotations;

namespace CoachBackend.Models;

public class MatchEvent
{
    public int Id { get; set; }
    
    [Required]
    public int CurrentMatchId { get; set; }
    
    [Required]
    public string EventType { get; set; } = ""; // substitution, goal, yellow_card, red_card, position_change
    
    public int? PlayerId { get; set; }
    public int? PlayerOutId { get; set; }
    public int? PlayerInId { get; set; }
    
    public int? FieldPositionId { get; set; }
    
    public int MatchMinute { get; set; }
    public int MatchSecond { get; set; }
    
    public string? Notes { get; set; }
    
    public DateTime EventTime { get; set; } = DateTime.UtcNow;
} 