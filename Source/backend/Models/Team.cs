using System.ComponentModel.DataAnnotations;

namespace CoachBackend.Models;

public class Team
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string DatabaseName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }

    public List<User> Users { get; set; } = new();
} 