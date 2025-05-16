using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoachBackend.Models;

public class Position
{
    [JsonIgnore]
    public int Id { get; set; }

    [Required(ErrorMessage = "Namn är obligatoriskt")]
    [StringLength(50, ErrorMessage = "Namn får inte vara längre än 50 tecken")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Förkortning är obligatoriskt")]
    [StringLength(5, ErrorMessage = "Förkortning får inte vara längre än 5 tecken")]
    public string Abbreviation { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kategori är obligatoriskt")]
    [StringLength(20, ErrorMessage = "Kategori får inte vara längre än 20 tecken")]
    public string Category { get; set; } = string.Empty; // Målvakt, Back, Mittfält, Anfall

    [Required(ErrorMessage = "Beskrivning är obligatorisk")]
    public string Description { get; set; } = string.Empty;
} 