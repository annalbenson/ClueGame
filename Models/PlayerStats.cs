using System.ComponentModel.DataAnnotations;

namespace ClueGame.Models;

public class PlayerStats
{
    [Key]
    public int Id { get; set; }
    public int AvgAccusations { get; set; }
    public int AvgStepsTaken { get; set; }
}