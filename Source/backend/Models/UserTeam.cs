namespace CoachBackend.Models;

public class UserTeam
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
} 