using System.ComponentModel.DataAnnotations;

namespace CoachBackend.Models;

public class FieldPosition
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = "";
    
    [Required]
    public string Abbreviation { get; set; } = "";
    
    [Required]
    public string Zone { get; set; } = ""; // defense, midfield, attack, goalkeeper, bench
    
    public int SortOrder { get; set; }
    
    public string? Description { get; set; }
} 