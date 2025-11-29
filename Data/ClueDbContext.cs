using System.Runtime.InteropServices;
using ClueGame.Models;
using ClueGame.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClueGame.Data;

public class ClueDbContext : DbContext
{
    public ClueDbContext(DbContextOptions<ClueDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Room Adjacency
        modelBuilder.Entity<RoomAdjacency>()
            .HasOne(rA => rA.RoomA)
            .WithMany()
            .HasForeignKey(rA => rA.RoomAId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RoomAdjacency>()
            .HasOne(rA => rA.RoomB)
            .WithMany()
            .HasForeignKey(rA => rA.RoomBId)
            .OnDelete(DeleteBehavior.Restrict);

        // Game
        modelBuilder.Entity<Game>()
            .HasOne(g => g.HumanCharacter)
            .WithMany()
            .HasForeignKey(g => g.HumanCharacterId)
            .OnDelete(DeleteBehavior.Restrict); // we don't want to delete cards if we delete a game or accusation

        modelBuilder.Entity<Game>()
            .HasOne(g => g.SolutionSuspect)
            .WithMany()
            .HasForeignKey(g => g.SolutionRoomId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Game>()
            .HasOne(g => g.SolutionWeapon)
            .WithMany()
            .HasForeignKey(g => g.SolutionWeaponId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Game>()
            .HasOne(g => g.SolutionRoom)
            .WithMany()
            .HasForeignKey(g => g.SolutionRoomId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Game>()
            .HasOne(g => g.CurrentTurnPlayer)
            .WithMany()
            .HasForeignKey(g => g.CurrentTurnPlayerId)
            .OnDelete(DeleteBehavior.Restrict);


        // Player
        modelBuilder.Entity<Player>()
            .HasOne(p => p.CharacterCard)
            .WithMany()
            .HasForeignKey(p => p.CharacterCardId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Player>()
            .HasOne(p => p.PlayerRoom)
            .WithMany()
            .HasForeignKey(p => p.PlayerRoomId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Player>()
            .HasOne(p => p.Game)
            .WithMany(g => g.Players)
            .HasForeignKey(p => p.GameId)
            .OnDelete(DeleteBehavior.Restrict);

        // Player Card
        modelBuilder.Entity<PlayerCard>()
            .HasOne(pc => pc.Player)
            .WithMany(p => p.Cards)
            .HasForeignKey(pc => pc.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PlayerCard>()
            .HasOne(pc => pc.Card)
            .WithMany()
            .HasForeignKey(pc => pc.CardId)
            .OnDelete(DeleteBehavior.Restrict);

        // Accusation
        modelBuilder.Entity<Accusation>()
            .HasOne(a => a.AccusingPlayer)
            .WithMany()
            .HasForeignKey(a => a.AccusingPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Accusation>()
            .HasOne(a => a.SuspectCard)
            .WithMany()
            .HasForeignKey(a => a.SuspectCardId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Accusation>()
            .HasOne(a => a.WeaponCard)
            .WithMany()
            .HasForeignKey(a => a.WeaponCardId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Accusation>()
           .HasOne(a => a.RoomCard)
           .WithMany()
           .HasForeignKey(a => a.RoomCardId)
           .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Accusation>()
            .HasOne(a => a.DisprovingPlayer)
            .WithMany()
            .HasForeignKey(a => a.DisprovingPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Accusation>()
            .HasOne(a => a.DisprovingCard)
            .WithMany()
            .HasForeignKey(a => a.DisprovingCardId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Accusation>()
            .HasOne(a => a.Game)
            .WithMany(g => g.Accusations)
            .HasForeignKey(a => a.GameId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public async Task SeedIfEmptyAsync()
    {
        if (!Cards.Any())
        {
            var seedCards = new List<Card>
            {
                // Suspects
                new() { Name = "Miss Scarlet", Type = CardType.Suspect },
                new() { Name = "Colonel Mustard", Type = CardType.Suspect },
                new() { Name = "Professor Plum", Type = CardType.Suspect },
                new() { Name = "Mrs. Peacock", Type = CardType.Suspect },
                new() { Name = "Mr. Green", Type = CardType.Suspect },
                new() { Name = "Mrs. White", Type = CardType.Suspect },

                // Weapons
                new() { Name = "Candlestick", Type = CardType.Weapon },
                new() { Name = "Revolver", Type = CardType.Weapon },
                new() { Name = "Rope", Type = CardType.Weapon },
                new() { Name = "Wrench", Type = CardType.Weapon },
                new() { Name = "Knife", Type = CardType.Weapon },
                new() { Name = "Lead Pipe", Type = CardType.Weapon },

                // Rooms
                new() { Name = "Kitchen", Type = CardType.Room },
                new() { Name = "Ballroom", Type = CardType.Room },
                new() { Name = "Conservatory", Type = CardType.Room },
                new() { Name = "Dining Room", Type = CardType.Room },
                new() { Name = "Billiard Room", Type = CardType.Room },
                new() { Name = "Library", Type = CardType.Room },
                new() { Name = "Lounge", Type = CardType.Room },
                new() { Name = "Hall", Type = CardType.Room },
                new() { Name = "Study", Type = CardType.Room }

            };

            Cards.AddRange(seedCards);
            await SaveChangesAsync();
        } // end if

        if (!RoomAdjacencies.Any())
        {
            var kitchen = Cards.First(c => c.Name == "Kitchen");
            var ballroom = Cards.First(c => c.Name == "Ballroom");
            var conservatory = Cards.First(c => c.Name == "Conservatory");
            var diningRoom = Cards.First(c => c.Name == "Dining Room");
            var billiardRoom = Cards.First(c => c.Name == "Billiard Room");
            var library = Cards.First(c => c.Name == "Library");
            var lounge = Cards.First(c => c.Name == "Lounge");
            var hall = Cards.First(c => c.Name == "Hall");
            var study = Cards.First(c => c.Name == "Study");

            RoomAdjacencies.AddRange(new[]{

                // kitchen
                new RoomAdjacency { RoomAId = kitchen.Id, RoomBId = diningRoom.Id }, // CCW
                new RoomAdjacency { RoomAId = kitchen.Id, RoomBId = ballroom.Id }, // CW
                new RoomAdjacency { RoomAId = kitchen.Id, RoomBId = study.Id }, // secret passage

                // ballroom
                new RoomAdjacency { RoomAId = ballroom.Id, RoomBId = kitchen.Id },
                new RoomAdjacency { RoomAId = ballroom.Id, RoomBId = conservatory.Id },

                // conservatory
                new RoomAdjacency { RoomAId = conservatory.Id, RoomBId = ballroom.Id },
                new RoomAdjacency { RoomAId = conservatory.Id, RoomBId = billiardRoom.Id },
                new RoomAdjacency { RoomAId = conservatory.Id, RoomBId = lounge.Id }, // secret passage

                // billiard room
                new RoomAdjacency { RoomAId = billiardRoom.Id, RoomBId = conservatory.Id },
                new RoomAdjacency { RoomAId = billiardRoom.Id, RoomBId = library.Id },

                // library room
                new RoomAdjacency { RoomAId = library.Id, RoomBId = billiardRoom.Id },
                new RoomAdjacency { RoomAId = library.Id, RoomBId = study.Id },

                // study
                new RoomAdjacency { RoomAId = study.Id, RoomBId = library.Id },
                new RoomAdjacency { RoomAId = study.Id, RoomBId = hall.Id },
                new RoomAdjacency { RoomAId = study.Id, RoomBId = kitchen.Id }, // secret passage

                // hall
                new RoomAdjacency { RoomAId = hall.Id, RoomBId = study.Id },
                new RoomAdjacency { RoomAId = hall.Id, RoomBId = lounge.Id },

                // lounge
                new RoomAdjacency { RoomAId = lounge.Id, RoomBId = hall.Id },
                new RoomAdjacency { RoomAId = lounge.Id, RoomBId = diningRoom.Id },
                new RoomAdjacency { RoomAId = lounge.Id, RoomBId = conservatory.Id }, // secret passage

                // dining room
                new RoomAdjacency { RoomAId = diningRoom.Id, RoomBId = lounge.Id },
                new RoomAdjacency { RoomAId = diningRoom.Id, RoomBId = kitchen.Id },

            });
            await SaveChangesAsync();

        } // end if
    }


    public DbSet<Card> Cards { get; set; }

    public DbSet<Player> Players { get; set; }
    public DbSet<PlayerCard> PlayerCards { get; set; }

    public DbSet<RoomAdjacency> RoomAdjacencies { get; set; }

    public DbSet<Game> Games { get; set; }
    public DbSet<Accusation> Accusations { get; set; }

    public DbSet<PlayerStats> PlayerStats { get; set; }
}