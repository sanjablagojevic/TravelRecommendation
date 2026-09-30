using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelRecommendation.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTravelItineraries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TravelItineraries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DestinationId = table.Column<int>(type: "int", nullable: false),
                    UserPreferenceId = table.Column<int>(type: "int", nullable: true),
                    RecommendationId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AiModel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelItineraries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TravelItineraries_Destinations_DestinationId",
                        column: x => x.DestinationId,
                        principalTable: "Destinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TravelItineraries_Recommendations_RecommendationId",
                        column: x => x.RecommendationId,
                        principalTable: "Recommendations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TravelItineraries_UserPreferences_UserPreferenceId",
                        column: x => x.UserPreferenceId,
                        principalTable: "UserPreferences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TravelItineraries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TravelItineraryDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TravelItineraryId = table.Column<int>(type: "int", nullable: false),
                    DayNumber = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelItineraryDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TravelItineraryDays_TravelItineraries_TravelItineraryId",
                        column: x => x.TravelItineraryId,
                        principalTable: "TravelItineraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TravelItineraryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TravelItineraryDayId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    TimeOfDay = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AttractionId = table.Column<int>(type: "int", nullable: true),
                    ActivityId = table.Column<int>(type: "int", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelItineraryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TravelItineraryItems_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TravelItineraryItems_Attractions_AttractionId",
                        column: x => x.AttractionId,
                        principalTable: "Attractions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TravelItineraryItems_TravelItineraryDays_TravelItineraryDayId",
                        column: x => x.TravelItineraryDayId,
                        principalTable: "TravelItineraryDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TravelItineraries_CreatedAt",
                table: "TravelItineraries",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TravelItineraries_DestinationId",
                table: "TravelItineraries",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelItineraries_RecommendationId",
                table: "TravelItineraries",
                column: "RecommendationId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelItineraries_UserId",
                table: "TravelItineraries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelItineraries_UserPreferenceId",
                table: "TravelItineraries",
                column: "UserPreferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelItineraryDays_TravelItineraryId_DayNumber",
                table: "TravelItineraryDays",
                columns: new[] { "TravelItineraryId", "DayNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TravelItineraryItems_ActivityId",
                table: "TravelItineraryItems",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelItineraryItems_AttractionId",
                table: "TravelItineraryItems",
                column: "AttractionId");

            migrationBuilder.CreateIndex(
                name: "IX_TravelItineraryItems_TravelItineraryDayId_Order",
                table: "TravelItineraryItems",
                columns: new[] { "TravelItineraryDayId", "Order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TravelItineraryItems");

            migrationBuilder.DropTable(
                name: "TravelItineraryDays");

            migrationBuilder.DropTable(
                name: "TravelItineraries");
        }
    }
}
