using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClueGame.Models;

public class RoomAdjacency
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(RoomA))]
    public int RoomAId { get; set; }

    public Card RoomA { get; set; }

    [Required]
    [ForeignKey(nameof(RoomB))]
    public int RoomBId { get; set; }

    public Card RoomB { get; set; }
}
