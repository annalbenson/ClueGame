using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ClueGame.Models;
using ClueGame.Models.Enums;
using ClueGame.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ClueGame.Controllers;

public class ClueController : Controller
{
    private readonly ClueDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public ClueController(ClueDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: /Clue/Play/{gameId}
    public async Task<IActionResult> Play(int gameId)
    {
        var game = await _context.Games
        .Include(g => g.HumanCharacter)
        .Include(g => g.Accusations)
            .ThenInclude(a => a.SuspectCard)
        .Include(g => g.Accusations)
            .ThenInclude(a => a.WeaponCard)
        .Include(g => g.Accusations)
            .ThenInclude(a => a.RoomCard)

       .Include(g => g.Accusations)
            .ThenInclude(a => a.DisprovingPlayer)
                .ThenInclude(p => p.CharacterCard)
        .Include(g => g.Accusations)
            .ThenInclude(a => a.DisprovingCard)

        .Include(g => g.Players)
            .ThenInclude(p => p.CharacterCard)
        .Include(g => g.Players)
            .ThenInclude(g => g.PlayerRoom)
        .Include(g => g.Players)
            .ThenInclude(p => p.Cards)
                .ThenInclude(pc => pc.Card)
        .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
        {
            return NotFound();
        }

        ViewBag.GameId = gameId;
        ViewBag.Suspects = await _context.Cards.Where(c => c.Type == CardType.Suspect).ToListAsync();
        ViewBag.Weapons = await _context.Cards.Where(c => c.Type == CardType.Weapon).ToListAsync();
        ViewBag.Rooms = await _context.Cards.Where(c => c.Type == CardType.Room).ToListAsync();

        var humanPlayer = game.Players.Where(p => p.CharacterCardId == game.HumanCharacterId).FirstOrDefault();

        var adjacentRoomIds = await _context.RoomAdjacencies
            .Where(a => a.RoomAId == humanPlayer.PlayerRoomId)
            .Select(a => a.RoomBId)
            .ToListAsync();

        ViewBag.AdjacentRooms = await _context.Cards
            .Where(c => adjacentRoomIds.Contains(c.Id))
            .ToListAsync();

        ViewBag.NpcPlayers = game.Players
            .Where(p => p.CharacterCardId != game.HumanCharacterId)
            .ToList();

        return View(game); // Views/Clue/Play.cshtml
    }

    //POST : /Clue/MoveRoom
    [HttpPost]
    public async Task<IActionResult> MoveRoom(int gameId, int targetRoomId)
    {
        var game = await _context.Games
        .Include(g => g.Players)
            .ThenInclude(p => p.PlayerRoom)
        .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
        {
            return NotFound();
        }

        var humanPlayer = game.CurrentTurnPlayer;

        humanPlayer.PlayerRoomId = targetRoomId;
        game.StepsTaken += 1;
        await _context.SaveChangesAsync();

        // advance turn
        await AdvanceTurnAndHandleNPCs(game.Id);

        return RedirectToAction("Play", new { gameId });
    }

    // POST: /Clue/SubmitGuess
    [HttpPost]
    public async Task<IActionResult> SubmitGuess(int gameId, int suspectId, int weaponId, int roomId)
    {
        var game = await _context.Games.Include(g => g.Players)
                                        .ThenInclude(p => p.Cards)
                                        .ThenInclude(pc => pc.Card)
                                        .Include(g => g.CurrentTurnPlayer)
                                        .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
        {
            return NotFound();
        }

        // move the named suspect to the guessed room
        var suspectedPlayer = game.Players.FirstOrDefault(p => p.CharacterCardId == suspectId);
        if (suspectedPlayer != null)
        {
            suspectedPlayer.PlayerRoomId = roomId;
        }

        var accusation = new Accusation
        {
            AccusingPlayerId = game.CurrentTurnPlayerId.Value,
            SuspectCardId = suspectId,
            WeaponCardId = weaponId,
            RoomCardId = roomId,
            Timestamp = DateTime.Now,
            GameId = gameId,
        };

        CheckAccusation(accusation, game);

        _context.Accusations.Add(accusation);
        await _context.SaveChangesAsync();

        if (accusation.IsCorrect != true)
        {
            // advance turn
            await AdvanceTurnAndHandleNPCs(game.Id);
        }

        return RedirectToAction("Play", new { gameId });

    }


    private async Task AdvanceTurnAndHandleNPCs(int gameId)
    {
        var game = await _context.Games.Include(g => g.Players)
                                        .ThenInclude(p => p.Cards)
                                        .ThenInclude(pc => pc.Card)
                                        .Include(g => g.Accusations)
                                        .ThenInclude(a => a.DisprovingCard)
                                        .Include(g => g.CurrentTurnPlayer)
                                        .FirstOrDefaultAsync(g => g.Id == gameId);

        var humanPlayer = game.Players.First(p => p.CharacterCardId == game.HumanCharacterId);

        AdvancePlayToNextPlayer(game);
        var currentPlayer = game.Players.First(p => p.Id == game.CurrentTurnPlayerId);

        while (currentPlayer.Id != humanPlayer.Id)
        {
            // if the current npc room is already disproven, then move
            var disprovedCards = game.Accusations.Select(a => a.DisprovingCard).Where(c => c != null).Distinct().ToList();
            var disprovedRooms = disprovedCards.Where(c => c.Type.Equals(CardType.Room)).ToList();
            if (disprovedRooms.Count != 0 && disprovedRooms.Contains(currentPlayer.PlayerRoom))
            {
                // have npc go clockwise by default
                var allAdjacencies = await _context.RoomAdjacencies.Include(rA => rA.RoomA).Include(rA => rA.RoomB).ToListAsync();
                var roomAdjacencies = allAdjacencies.Where(rA => rA.RoomAId == currentPlayer.PlayerRoomId).ToList();

                currentPlayer.PlayerRoomId = roomAdjacencies[1].RoomBId; // CW option
                await _context.SaveChangesAsync();
            }
            else
            {
                // else, make a guess
                var accusation = await GenerateNPCGuess(game, currentPlayer);

                // move the named suspect to the npc's room
                var suspectedPlayer = game.Players.FirstOrDefault(p => p.CharacterCardId == accusation.SuspectCardId);
                if (suspectedPlayer != null)
                {
                    suspectedPlayer.PlayerRoomId = accusation.RoomCardId;
                }

                // check if that guess is correct or disprove
                CheckAccusation(accusation, game);

                // save
                _context.Accusations.Add(accusation);
                await _context.SaveChangesAsync();
            }

            // advance turn again
            AdvancePlayToNextPlayer(game);
            currentPlayer = game.Players.First(p => p.Id == game.CurrentTurnPlayerId);
        }

        await _context.SaveChangesAsync();
    }

    private void CheckAccusation(Accusation accusation, Game game)
    {

        bool isCorrect = game.SolutionSuspectId == accusation.SuspectCardId && game.SolutionWeaponId == accusation.WeaponCardId && game.SolutionRoomId == accusation.RoomCardId;

        if (isCorrect)
        {
            // todo: Victory Code Here
            accusation.IsCorrect = true;
            game.FinishedAt = DateTime.Now;
        }
        else
        {
            accusation.IsCorrect = false;
            // Who can disprove?
            var currentPlayer = game.CurrentTurnPlayer;
            var otherPlayersInOrder = game.Players.Where(p => p.Id != currentPlayer.Id).OrderBy(p => p.Id).ToList();

            foreach (var player in otherPlayersInOrder)
            {
                var matchingCard = player.Cards.Select(pc => pc.Card).FirstOrDefault(c => c.Id == accusation.SuspectCardId || c.Id == accusation.WeaponCardId || c.Id == accusation.RoomCardId);
                if (matchingCard != null)
                {
                    accusation.DisprovingPlayerId = player.Id;
                    accusation.DisprovingCardId = matchingCard.Id;
                    break;
                }
            }
        }

    }

    private void AdvancePlayToNextPlayer(Game game)
    {
        var orderedPlayers = game.Players.OrderBy(p => p.Id).ToList();
        int currentIndex = orderedPlayers.FindIndex(p => p.Id == game.CurrentTurnPlayerId);
        int nextIndex = (currentIndex + 1) % orderedPlayers.Count;
        game.CurrentTurnPlayerId = orderedPlayers[nextIndex].Id;
    }

    // makes a random npc guess 
    private async Task<Accusation> GenerateNPCGuess(Game game, Player npc)
    {
        var allCards = await _context.Cards.ToListAsync();

        var npcHandIds = npc.Cards.Select(c => c.CardId).ToList();
        var disprovedCardIds = game.Accusations.Select(a => a.DisprovingCardId).Distinct().ToList();
        var guessableCards = allCards.Where(c => !npcHandIds.Contains(c.Id) && !disprovedCardIds.Contains(c.Id)).ToList();

        // select accusation cards at random
        var random = new Random();

        var suspect = guessableCards.Where(c => c.Type.Equals(CardType.Suspect)).OrderBy(_ => random.Next()).Take(1).First();
        var weapon = guessableCards.Where(c => c.Type.Equals(CardType.Weapon)).OrderBy(_ => random.Next()).Take(1).First();

        var accusation = new Accusation
        {
            AccusingPlayerId = npc.Id,
            SuspectCardId = suspect.Id,
            WeaponCardId = weapon.Id,
            RoomCardId = npc.PlayerRoomId, // constrains to their current room
            Timestamp = DateTime.Now,
            GameId = game.Id,
        };

        return accusation;

    }

}