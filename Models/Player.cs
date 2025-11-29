using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Identity.Client;

namespace ClueGame.Models;

public class Player
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }

    [Required]
    [ForeignKey(nameof(CharacterCard))]
    public int CharacterCardId { get; set; }
    public Card CharacterCard { get; set; }

    [Required]
    [ForeignKey(nameof(PlayerRoom))]
    public int PlayerRoomId { get; set; }
    public Card PlayerRoom { get; set; }

    [Required]
    [ForeignKey(nameof(Game))]
    public int GameId { get; set; }
    public Game Game { get; set; }

    public List<PlayerCard> Cards { get; set; } = new();

}