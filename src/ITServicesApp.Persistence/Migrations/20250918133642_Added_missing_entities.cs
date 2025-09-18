using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITServicesApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Added_missing_entities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstimatedDurationMinutes",
                table: "BookingDrafts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "BookingDraftItems",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedDurationMinutes",
                table: "BookingDrafts");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "BookingDraftItems");
        }
    }
}
