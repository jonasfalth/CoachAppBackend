using System.ComponentModel.DataAnnotations;

namespace CoachBackend.Models;

public class PlayerPosition
{
    public int Id { get; set; }
    
    [Required]
    public int CurrentMatchId { get; set; }
    
    [Required]
    public int PlayerId { get; set; }
    
    [Required]
    public int FieldPositionId { get; set; }
    
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    
    public bool IsStarting { get; set; } = false;
    public bool IsActive { get; set; } = true;
} 