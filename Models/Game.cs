using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClueGame.Models.Enums;

namespace ClueGame.Models;

public class Game
{
    [Key]
    public int Id { get; set; }

    // Human Player State
    [Required]
    [ForeignKey(nameof(HumanCharacter))]
    public int HumanCharacterId { get; set; }
    public Card HumanCharacter { get; set; }

    // Solution 
    [Required]
    [ForeignKey(nameof(SolutionSuspect))]
    public int SolutionSuspectId { get; set; }

    public Card SolutionSuspect { get; set; }

    [Required]
    [ForeignKey(nameof(SolutionWeapon))]
    public int SolutionWeaponId { get; set; }

    public Card SolutionWeapon { get; set; }

    [Required]
    [ForeignKey(nameof(SolutionRoom))]
    public int SolutionRoomId { get; set; }

    public Card SolutionRoom { get; set; }

    [Required]
    public DateTime StartedAt { get; set; } = DateTime.Now;

    public DateTime? FinishedAt { get; set; }

    [ForeignKey(nameof(CurrentTurnPlayer))]
    public int? CurrentTurnPlayerId { get; set; }
    public Player? CurrentTurnPlayer { get; set; }


    public int StepsTaken { get; set; }

    public List<Player> Players { get; set; } = new();

    public List<Accusation> Accusations { get; set; } = new();
}
