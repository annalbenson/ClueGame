using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClueGame.Models;

public class Accusation
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(AccusingPlayer))]
    public int AccusingPlayerId { get; set; }
    public Player AccusingPlayer { get; set; }

    [Required]
    [ForeignKey(nameof(SuspectCard))]
    public int SuspectCardId { get; set; }
    public Card SuspectCard { get; set; }

    [Required]
    [ForeignKey(nameof(WeaponCard))]
    public int WeaponCardId { get; set; }

    public Card WeaponCard { get; set; }

    [Required]
    [ForeignKey(nameof(RoomCard))]
    public int RoomCardId { get; set; }

    public Card RoomCard { get; set; }

    [ForeignKey(nameof(DisprovingPlayer))]
    public int? DisprovingPlayerId { get; set; }
    public Player? DisprovingPlayer { get; set; }

    [ForeignKey(nameof(DisprovingCard))]
    public int? DisprovingCardId { get; set; }
    public Card? DisprovingCard { get; set; }

    [Required]
    public bool IsCorrect { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.Now;

    [Required]
    [ForeignKey(nameof(Game))]
    public int GameId { get; set; }
    public Game Game { get; set; }
}
