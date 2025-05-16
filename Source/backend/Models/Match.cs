using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoachBackend.Models;

public class Match
{
    [JsonIgnore]
    public int Id { get; set; }

    [Required(ErrorMessage = "Datum är obligatoriskt")]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Motståndarlag är obligatoriskt")]
    [StringLength(100, ErrorMessage = "Motståndarlag får inte vara längre än 100 tecken")]
    public string Opponent { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hemma/borta är obligatoriskt")]
    public bool HomeGame { get; set; }

    public string? Result { get; set; }

    public string? Notes { get; set; }
} 