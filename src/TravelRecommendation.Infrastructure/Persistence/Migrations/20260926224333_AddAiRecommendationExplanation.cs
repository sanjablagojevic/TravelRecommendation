using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelRecommendation.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAiRecommendationExplanation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiExplanation",
                table: "RecommendationItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiExplanation",
                table: "RecommendationItems");
        }
    }
}
