# Clue Game

A web-based implementation of the classic Clue board game, built with ASP.NET Core MVC and Entity Framework Core.

## Features

- **Board map** — 9-room grid layout with secret passages between Kitchen↔Study and Conservatory↔Lounge
- **NPC opponents** — 3 AI players who move and make accusations based on their own deduced knowledge
- **Turn summary overlay** — After each action, a summary shows what you did and how every NPC responded before you continue
- **Disproval privacy** — Disproval cards are hidden for accusations you didn't make, matching real Clue rules
- **Game persistence** — Save and resume games; home screen shows accusation count for each saved game
- **Player stats** — Tracks average accusations and steps taken across your last 10 completed games

## Tech Stack

- **ASP.NET Core MVC** (.NET 9)
- **Entity Framework Core 9** with SQL Server LocalDB
- **Razor views** with Bootstrap

## Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- SQL Server LocalDB (included with Visual Studio)

### Run locally

```bash
git clone https://github.com/annalbenson/ClueGame.git
cd ClueGame
dotnet run
```

Then open [http://localhost:5173](http://localhost:5173) in your browser.

The database is created and seeded automatically on first run.

## How to Play

1. Select a character and start a new game (or resume a saved one)
2. On your turn, move to an adjacent room or use a secret passage
3. Once in a room, make an accusation naming a suspect, weapon, and room
4. Other players attempt to disprove your accusation — if someone can, they show you one matching card
5. Use the guess history and your known cards to deduce the solution
6. When you're confident, accuse correctly to win!
