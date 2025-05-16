using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoachBackend.Models;

public class Gameplay
{
    [JsonIgnore]
    public int Id { get; set; }

    [Required]
    public int MatchId { get; set; }

    [Required]
    public int PlayerId { get; set; }

    public int? MinutesPlayed { get; set; }

    [Range(0, int.MaxValue)]
    public int Goals { get; set; }

    [Range(0, int.MaxValue)]
    public int Assists { get; set; }

    [Range(0, int.MaxValue)]
    public int YellowCards { get; set; }

    [Range(0, int.MaxValue)]
    public int RedCards { get; set; }

    [Range(0, 10)]
    public double? Rating { get; set; }

    public string? Notes { get; set; }
} 