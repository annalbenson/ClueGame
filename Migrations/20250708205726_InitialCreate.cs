using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClueGame.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AvgAccusations = table.Column<int>(type: "int", nullable: false),
                    AvgStepsTaken = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerStats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoomAdjacencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomAId = table.Column<int>(type: "int", nullable: false),
                    RoomBId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomAdjacencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomAdjacencies_Cards_RoomAId",
                        column: x => x.RoomAId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoomAdjacencies_Cards_RoomBId",
                        column: x => x.RoomBId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Accusations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccusingPlayerId = table.Column<int>(type: "int", nullable: false),
                    SuspectCardId = table.Column<int>(type: "int", nullable: false),
                    WeaponCardId = table.Column<int>(type: "int", nullable: false),
                    RoomCardId = table.Column<int>(type: "int", nullable: false),
                    DisprovingPlayerId = table.Column<int>(type: "int", nullable: true),
                    DisprovingCardId = table.Column<int>(type: "int", nullable: true),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GameId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accusations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accusations_Cards_DisprovingCardId",
                        column: x => x.DisprovingCardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Accusations_Cards_RoomCardId",
                        column: x => x.RoomCardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Accusations_Cards_SuspectCardId",
                        column: x => x.SuspectCardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Accusations_Cards_WeaponCardId",
                        column: x => x.WeaponCardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HumanCharacterId = table.Column<int>(type: "int", nullable: false),
                    SolutionSuspectId = table.Column<int>(type: "int", nullable: false),
                    SolutionWeaponId = table.Column<int>(type: "int", nullable: false),
                    SolutionRoomId = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentTurnPlayerId = table.Column<int>(type: "int", nullable: true),
                    StepsTaken = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Games_Cards_HumanCharacterId",
                        column: x => x.HumanCharacterId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Games_Cards_SolutionRoomId",
                        column: x => x.SolutionRoomId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Games_Cards_SolutionSuspectId",
                        column: x => x.SolutionSuspectId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Games_Cards_SolutionWeaponId",
                        column: x => x.SolutionWeaponId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CharacterCardId = table.Column<int>(type: "int", nullable: false),
                    PlayerRoomId = table.Column<int>(type: "int", nullable: false),
                    GameId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_Cards_CharacterCardId",
                        column: x => x.CharacterCardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Players_Cards_PlayerRoomId",
                        column: x => x.PlayerRoomId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Players_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    CardId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerCards_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlayerCards_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accusations_AccusingPlayerId",
                table: "Accusations",
                column: "AccusingPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Accusations_DisprovingCardId",
                table: "Accusations",
                column: "DisprovingCardId");

            migrationBuilder.CreateIndex(
                name: "IX_Accusations_DisprovingPlayerId",
                table: "Accusations",
                column: "DisprovingPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Accusations_GameId",
                table: "Accusations",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Accusations_RoomCardId",
                table: "Accusations",
                column: "RoomCardId");

            migrationBuilder.CreateIndex(
                name: "IX_Accusations_SuspectCardId",
                table: "Accusations",
                column: "SuspectCardId");

            migrationBuilder.CreateIndex(
                name: "IX_Accusations_WeaponCardId",
                table: "Accusations",
                column: "WeaponCardId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_CurrentTurnPlayerId",
                table: "Games",
                column: "CurrentTurnPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_HumanCharacterId",
                table: "Games",
                column: "HumanCharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_SolutionRoomId",
                table: "Games",
                column: "SolutionRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_SolutionSuspectId",
                table: "Games",
                column: "SolutionSuspectId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_SolutionWeaponId",
                table: "Games",
                column: "SolutionWeaponId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerCards_CardId",
                table: "PlayerCards",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerCards_PlayerId",
                table: "PlayerCards",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_CharacterCardId",
                table: "Players",
                column: "CharacterCardId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_GameId",
                table: "Players",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_PlayerRoomId",
                table: "Players",
                column: "PlayerRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomAdjacencies_RoomAId",
                table: "RoomAdjacencies",
                column: "RoomAId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomAdjacencies_RoomBId",
                table: "RoomAdjacencies",
                column: "RoomBId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accusations_Games_GameId",
                table: "Accusations",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Accusations_Players_AccusingPlayerId",
                table: "Accusations",
                column: "AccusingPlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Accusations_Players_DisprovingPlayerId",
                table: "Accusations",
                column: "DisprovingPlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Players_CurrentTurnPlayerId",
                table: "Games",
                column: "CurrentTurnPlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Cards_HumanCharacterId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_Cards_SolutionRoomId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_Cards_SolutionSuspectId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_Cards_SolutionWeaponId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Players_Cards_CharacterCardId",
                table: "Players");

            migrationBuilder.DropForeignKey(
                name: "FK_Players_Cards_PlayerRoomId",
                table: "Players");

            migrationBuilder.DropForeignKey(
                name: "FK_Players_Games_GameId",
                table: "Players");

            migrationBuilder.DropTable(
                name: "Accusations");

            migrationBuilder.DropTable(
                name: "PlayerCards");

            migrationBuilder.DropTable(
                name: "PlayerStats");

            migrationBuilder.DropTable(
                name: "RoomAdjacencies");

            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
