using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoachBackend.Models;

public class Player
{
    [JsonIgnore]
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Namn är obligatoriskt")]
    [StringLength(100, ErrorMessage = "Namn får inte vara längre än 100 tecken")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Födelseår är obligatoriskt")]
    [Range(1900, 2100, ErrorMessage = "Födelseår måste vara mellan 1900 och 2100")]
    public int BirthYear { get; set; }

    [Required(ErrorMessage = "Tröjnummer är obligatoriskt")]
    [Range(1, 99, ErrorMessage = "Tröjnummer måste vara mellan 1 och 99")]
    public int JerseyNumber { get; set; }

    public string? ProfileImage { get; set; }

    [Required(ErrorMessage = "Position är obligatorisk")]
    public int PositionId { get; set; }

    public string? Notes { get; set; }
} 