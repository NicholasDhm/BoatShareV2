using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace boat_share.Migrations
{
    /// <inheritdoc />
    public partial class AddBoatImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Boats",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            // Seed data updates removed - only adding the column
            // The ImageUrl will default to null for existing boats
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Boats");
        }
    }
}
