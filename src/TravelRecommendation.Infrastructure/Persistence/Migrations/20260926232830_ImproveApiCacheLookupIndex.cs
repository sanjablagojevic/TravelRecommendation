using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelRecommendation.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ImproveApiCacheLookupIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ApiCaches_Provider_Action_RequestKey",
                table: "ApiCaches",
                columns: new[] { "Provider", "Action", "RequestKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApiCaches_Provider_Action_RequestKey",
                table: "ApiCaches");
        }
    }
}
