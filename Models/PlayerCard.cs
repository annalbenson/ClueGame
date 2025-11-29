using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClueGame.Models;

public class PlayerCard
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(Player))]
    public int PlayerId { get; set; }
    public Player Player { get; set; }

    [Required]
    [ForeignKey(nameof(Card))]
    public int CardId { get; set; }
    public Card Card { get; set; }
}