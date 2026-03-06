using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ClueGame.Models;
using ClueGame.Models.Enums;
using ClueGame.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace ClueGame.Controllers;

public class HomeController : Controller
{
    private readonly ClueDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ClueDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET
    public async Task<IActionResult> Index()
    {
        await _context.SeedIfEmptyAsync(); // seed db if empty

        // character selection
        ViewBag.Suspects = await _context.Cards.Where(c => c.Type == CardType.Suspect).ToListAsync();

        // load game selection
        var recentActiveGames = await _context.Games.Where(g => g.FinishedAt == null).Include(g => g.Accusations).OrderByDescending(g => g.StartedAt).Take(3).ToListAsync();
        ViewBag.RecentGames = recentActiveGames;

        // stats calculation
        var recentFinishedGames = await _context.Games
            .Where(g => g.FinishedAt != null)
            .Include(g => g.Players)
            .Include(g => g.Accusations)
            .OrderByDescending(g => g.StartedAt)
            .Take(10)
            .ToListAsync();

        int finishedCount = recentFinishedGames.Count;
        int avgAccusations = 0;
        int avgStepsTaken = 0;

        if (finishedCount > 0)
        {
            avgAccusations = recentFinishedGames.Select(g => {
                var humanPlayer = g.Players.FirstOrDefault(p => p.CharacterCardId == g.HumanCharacterId);
                return humanPlayer != null ? g.Accusations.Count(a => a.AccusingPlayerId == humanPlayer.Id) : 0;
            }).Sum() / finishedCount;

            avgStepsTaken = recentFinishedGames.Select(g => g.StepsTaken).Sum() / finishedCount;
        }

        ViewBag.FinishedGameCount = finishedCount;
        ViewBag.PlayerStats = new PlayerStats { AvgAccusations = avgAccusations, AvgStepsTaken = avgStepsTaken };

        return View();
    }

    // POST: /StartGame
    [HttpPost]
    public async Task<IActionResult> StartGame(int playerCharacterId)
    {
        await _context.SeedIfEmptyAsync(); // seed db if empty

        var allCards = await _context.Cards.ToListAsync();
        var suspects = await _context.Cards.Where(c => c.Type == CardType.Suspect).ToListAsync();
        var weapons = await _context.Cards.Where(c => c.Type == CardType.Weapon).ToListAsync();
        var rooms = await _context.Cards.Where(c => c.Type == CardType.Room).ToListAsync();

        // select solution cards at random
        var random = new Random();

        var solutionSuspect = suspects[random.Next(suspects.Count)];
        var solutionWeapon = weapons[random.Next(weapons.Count)];
        var solutionRoom = rooms[random.Next(rooms.Count)];

        var game = new Game()
        {
            HumanCharacterId = playerCharacterId,
            SolutionSuspectId = solutionSuspect.Id,
            SolutionWeaponId = solutionWeapon.Id,
            SolutionRoomId = solutionRoom.Id,
            StartedAt = DateTime.Now,
            StepsTaken = 0,
        };

        _context.Games.Add(game);
        await _context.SaveChangesAsync(); // Game will now have an Id

        // setup other players
        var playerCharacter = suspects.Where(c => c.Id == playerCharacterId).FirstOrDefault();

        // choose three other players
        var allCharacters = suspects;
        var npcCharacters = allCharacters.Where(c => c.Id != playerCharacterId).OrderBy(_ => random.Next()).Take(3).ToList();
        ViewBag.NpcCharacters = npcCharacters; // for ease of display

        var players = new List<Player>();

        var humanPlayer = new Player
        {
            Name = playerCharacter.Name,
            CharacterCardId = playerCharacterId,
            PlayerRoomId = rooms[random.Next(rooms.Count)].Id, // random room start for now
            GameId = game.Id
        };
        players.Add(humanPlayer);

        foreach (var npc in npcCharacters)
        {
            players.Add(new Player
            {
                Name = npc.Name,
                CharacterCardId = npc.Id,
                PlayerRoomId = rooms[random.Next(rooms.Count)].Id, // random room start for now
                GameId = game.Id
            });
        }

        // deal cards
        var dealableCards = allCards.Except(new[] { solutionSuspect, solutionWeapon, solutionRoom }).OrderBy(_ => random.Next()).ToList();
        int p = 0;
        foreach (var card in dealableCards)
        {
            players[p % players.Count].Cards.Add(new PlayerCard { CardId = card.Id });
            p++;
        }

        _context.Players.AddRange(players);
        await _context.SaveChangesAsync(); // Player IDs assigned

        // human player goes first
        game.CurrentTurnPlayerId = humanPlayer.Id;
        await _context.SaveChangesAsync();

        // route: /Clue/Play/{gameId}
        return RedirectToAction("Play", "Clue", new { gameId = game.Id });
    }

    // POST /LoadGame
    [HttpPost]
    public async Task<IActionResult> LoadGame(int gameId)
    {
        return RedirectToAction("Play", "Clue", new { gameId = gameId });
    }


    // POST /DeleteGame
    [HttpPost]
    public async Task<IActionResult> DeleteGame(int gameId)
    {
        var game = await _context.Games.Include(g => g.Players)
                                        .ThenInclude(p => p.Cards)
                                        .Include(g => g.Accusations).FirstOrDefaultAsync(g => g.Id == gameId);
                                        

        if (game == null)
        {
            return NotFound();
        }

        // due to all the foreign keys, we have DeleteBehavior.Restrict set in ClueDbContext
        // therefore, here we have to explicity delete the innards of game
        
        foreach (var cards in _context.PlayerCards) {
            _context.PlayerCards.RemoveRange(cards);
        }

        _context.Players.RemoveRange(game.Players);
        _context.Accusations.RemoveRange(game.Accusations);

        _context.Games.Remove(game);
        await _context.SaveChangesAsync();


        return RedirectToAction("Index");
    }

    // GET
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
