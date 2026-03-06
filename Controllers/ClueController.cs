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
        var game = await LoadGameForPlay(gameId);
        if (game == null) return NotFound();

        await PopulatePlayViewBag(game, gameId);
        return View(game);
    }

    // POST: /Clue/MoveRoom
    [HttpPost]
    public async Task<IActionResult> MoveRoom(int gameId, int targetRoomId)
    {
        var game = await _context.Games
            .Include(g => g.Players).ThenInclude(p => p.PlayerRoom)
            .Include(g => g.Players).ThenInclude(p => p.CharacterCard)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null) return NotFound();

        var humanPlayer = game.CurrentTurnPlayer;
        humanPlayer.PlayerRoomId = targetRoomId;
        game.StepsTaken += 1;
        await _context.SaveChangesAsync();

        var newRoom = await _context.Cards.FindAsync(targetRoomId);
        var turnEvents = new List<string>
        {
            $"{humanPlayer.CharacterCard.Name} moved to the {newRoom!.Name}"
        };

        var npcEvents = await AdvanceTurnAndHandleNPCs(game.Id);
        turnEvents.AddRange(npcEvents);

        var reloadedGame = await LoadGameForPlay(gameId);
        await PopulatePlayViewBag(reloadedGame!, gameId);
        ViewBag.TurnEvents = turnEvents;
        ViewBag.AccusationCorrect = false;

        return View("Play", reloadedGame);
    }

    // POST: /Clue/SubmitGuess
    [HttpPost]
    public async Task<IActionResult> SubmitGuess(int gameId, int suspectId, int weaponId, int roomId)
    {
        var game = await _context.Games
            .Include(g => g.Players).ThenInclude(p => p.Cards).ThenInclude(pc => pc.Card)
            .Include(g => g.Players).ThenInclude(p => p.CharacterCard)
            .Include(g => g.CurrentTurnPlayer)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null) return NotFound();

        // move named suspect to guessed room
        var suspectedPlayer = game.Players.FirstOrDefault(p => p.CharacterCardId == suspectId);
        if (suspectedPlayer != null)
            suspectedPlayer.PlayerRoomId = roomId;

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

        // Build human turn event string
        var allCards = await _context.Cards.ToListAsync();
        var suspectCard = allCards.First(c => c.Id == suspectId);
        var weaponCard = allCards.First(c => c.Id == weaponId);
        var roomCard = allCards.First(c => c.Id == roomId);
        var accuserName = game.Players.First(p => p.Id == accusation.AccusingPlayerId).CharacterCard.Name;

        var turnEvents = new List<string>
        {
            $"{accuserName} accused {suspectCard.Name} in the {roomCard.Name} with the {weaponCard.Name}"
        };

        if (accusation.IsCorrect != true)
        {
            var npcEvents = await AdvanceTurnAndHandleNPCs(gameId);
            turnEvents.AddRange(npcEvents);
        }

        var reloadedGame = await LoadGameForPlay(gameId);
        await PopulatePlayViewBag(reloadedGame!, gameId);
        ViewBag.TurnEvents = turnEvents;
        ViewBag.AccusationCorrect = accusation.IsCorrect == true;

        return View("Play", reloadedGame);
    }

    private async Task<Game?> LoadGameForPlay(int gameId)
    {
        return await _context.Games
            .Include(g => g.HumanCharacter)
            .Include(g => g.Accusations).ThenInclude(a => a.SuspectCard)
            .Include(g => g.Accusations).ThenInclude(a => a.WeaponCard)
            .Include(g => g.Accusations).ThenInclude(a => a.RoomCard)
            .Include(g => g.Accusations).ThenInclude(a => a.DisprovingPlayer).ThenInclude(p => p.CharacterCard)
            .Include(g => g.Accusations).ThenInclude(a => a.DisprovingCard)
            .Include(g => g.Players).ThenInclude(p => p.CharacterCard)
            .Include(g => g.Players).ThenInclude(p => p.PlayerRoom)
            .Include(g => g.Players).ThenInclude(p => p.Cards).ThenInclude(pc => pc.Card)
            .FirstOrDefaultAsync(g => g.Id == gameId);
    }

    private async Task PopulatePlayViewBag(Game game, int gameId)
    {
        ViewBag.GameId = gameId;
        ViewBag.Suspects = await _context.Cards.Where(c => c.Type == CardType.Suspect).ToListAsync();
        ViewBag.Weapons = await _context.Cards.Where(c => c.Type == CardType.Weapon).ToListAsync();
        ViewBag.Rooms = await _context.Cards.Where(c => c.Type == CardType.Room).ToListAsync();

        var humanPlayer = game.Players.First(p => p.CharacterCardId == game.HumanCharacterId);
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
    }

    private async Task<List<string>> AdvanceTurnAndHandleNPCs(int gameId)
    {
        var turnEvents = new List<string>();

        var game = await _context.Games
            .Include(g => g.Players).ThenInclude(p => p.Cards).ThenInclude(pc => pc.Card)
            .Include(g => g.Players).ThenInclude(p => p.CharacterCard)
            .Include(g => g.Players).ThenInclude(p => p.PlayerRoom)
            .Include(g => g.Accusations).ThenInclude(a => a.DisprovingCard)
            .Include(g => g.CurrentTurnPlayer)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        var allCards = await _context.Cards.ToListAsync();
        var humanPlayer = game.Players.First(p => p.CharacterCardId == game.HumanCharacterId);

        AdvancePlayToNextPlayer(game);
        var currentPlayer = game.Players.First(p => p.Id == game.CurrentTurnPlayerId);

        while (currentPlayer.Id != humanPlayer.Id)
        {
            var shownToCurrentNpc = game.Accusations
                .Where(a => a.AccusingPlayerId == currentPlayer.Id && a.DisprovingCard != null)
                .Select(a => a.DisprovingCard)
                .Distinct()
                .ToList();
            var disprovedRooms = shownToCurrentNpc.Where(c => c.Type.Equals(CardType.Room)).ToList();

            if (disprovedRooms.Count != 0 && disprovedRooms.Contains(currentPlayer.PlayerRoom))
            {
                var allAdjacencies = await _context.RoomAdjacencies.Include(rA => rA.RoomB).ToListAsync();
                var roomAdjacencies = allAdjacencies.Where(rA => rA.RoomAId == currentPlayer.PlayerRoomId).ToList();

                var newRoom = allCards.First(c => c.Id == roomAdjacencies[1].RoomBId);
                currentPlayer.PlayerRoomId = roomAdjacencies[1].RoomBId;
                await _context.SaveChangesAsync();

                turnEvents.Add($"{currentPlayer.CharacterCard.Name} moved to the {newRoom.Name}");
            }
            else
            {
                var accusation = GenerateNPCGuess(game, currentPlayer, allCards);

                var suspectedPlayer = game.Players.FirstOrDefault(p => p.CharacterCardId == accusation.SuspectCardId);
                if (suspectedPlayer != null)
                    suspectedPlayer.PlayerRoomId = accusation.RoomCardId;

                CheckAccusation(accusation, game);
                _context.Accusations.Add(accusation);
                await _context.SaveChangesAsync();

                var suspectCard = allCards.First(c => c.Id == accusation.SuspectCardId);
                var weaponCard = allCards.First(c => c.Id == accusation.WeaponCardId);
                var roomCard = allCards.First(c => c.Id == accusation.RoomCardId);
                turnEvents.Add($"{currentPlayer.CharacterCard.Name} accused {suspectCard.Name} in the {roomCard.Name} with the {weaponCard.Name}");
            }

            AdvancePlayToNextPlayer(game);
            currentPlayer = game.Players.First(p => p.Id == game.CurrentTurnPlayerId);
        }

        await _context.SaveChangesAsync();
        return turnEvents;
    }

    private void CheckAccusation(Accusation accusation, Game game)
    {
        bool isCorrect = game.SolutionSuspectId == accusation.SuspectCardId
                      && game.SolutionWeaponId == accusation.WeaponCardId
                      && game.SolutionRoomId == accusation.RoomCardId;

        if (isCorrect)
        {
            // todo: Victory Code Here
            accusation.IsCorrect = true;
            game.FinishedAt = DateTime.Now;
        }
        else
        {
            accusation.IsCorrect = false;
            var currentPlayer = game.CurrentTurnPlayer;
            var otherPlayersInOrder = game.Players.Where(p => p.Id != currentPlayer.Id).OrderBy(p => p.Id).ToList();

            foreach (var player in otherPlayersInOrder)
            {
                var matchingCard = player.Cards.Select(pc => pc.Card)
                    .FirstOrDefault(c => c.Id == accusation.SuspectCardId || c.Id == accusation.WeaponCardId || c.Id == accusation.RoomCardId);
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

    private Accusation GenerateNPCGuess(Game game, Player npc, List<Card> allCards)
    {
        var npcHandIds = npc.Cards.Select(c => c.CardId).ToList();
        var shownToNpcIds = game.Accusations
            .Where(a => a.AccusingPlayerId == npc.Id && a.DisprovingCardId != null)
            .Select(a => a.DisprovingCardId)
            .Distinct()
            .ToList();
        var eliminatedIds = npcHandIds.Concat(shownToNpcIds).ToHashSet();
        var guessableCards = allCards.Where(c => !eliminatedIds.Contains(c.Id)).ToList();

        var random = new Random();
        var suspect = guessableCards.Where(c => c.Type.Equals(CardType.Suspect)).OrderBy(_ => random.Next()).First();
        var weapon = guessableCards.Where(c => c.Type.Equals(CardType.Weapon)).OrderBy(_ => random.Next()).First();

        return new Accusation
        {
            AccusingPlayerId = npc.Id,
            SuspectCardId = suspect.Id,
            WeaponCardId = weapon.Id,
            RoomCardId = npc.PlayerRoomId,
            Timestamp = DateTime.Now,
            GameId = game.Id,
        };
    }
}
