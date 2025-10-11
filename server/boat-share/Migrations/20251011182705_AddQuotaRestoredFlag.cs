using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace boat_share.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotaRestoredFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "QuotaRestored",
                table: "Reservations",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuotaRestored",
                table: "Reservations");
        }
    }
}
