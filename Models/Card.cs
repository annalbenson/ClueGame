using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClueGame.Models.Enums;

namespace ClueGame.Models;

public class Card
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public CardType Type { get; set; }

    [NotMapped]
    public string ImageFileName => $"{Name.Replace(".", "").Replace(" ", "")}.jpg";
}
