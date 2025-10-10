using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace boat_share.Migrations
{
    /// <inheritdoc />
    public partial class AddLegacyStatusAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add composite index for user queries with status filtering
            migrationBuilder.CreateIndex(
                name: "IX_Reservations_UserId_Status_StartTime",
                table: "Reservations",
                columns: new[] { "UserId", "Status", "StartTime" });

            // Add composite index for user query optimization
            migrationBuilder.CreateIndex(
                name: "IX_Reservations_UserId_Status_Type_EndTime",
                table: "Reservations",
                columns: new[] { "UserId", "Status", "ReservationType", "EndTime" });

            // Add index for efficient archival queries (finding past reservations)
            migrationBuilder.CreateIndex(
                name: "IX_Reservations_EndTime_Status",
                table: "Reservations",
                columns: new[] { "EndTime", "Status" });

            // Add index for boat + status queries (calendar filtering)
            migrationBuilder.CreateIndex(
                name: "IX_Reservations_BoatId_Status_StartTime",
                table: "Reservations",
                columns: new[] { "BoatId", "Status", "StartTime" });

            // Drop old less efficient indexes
            migrationBuilder.DropIndex(
                name: "IX_Reservations_UserId_EndTime",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_UserId_ReservationType",
                table: "Reservations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop new indexes
            migrationBuilder.DropIndex(
                name: "IX_Reservations_UserId_Status_StartTime",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_UserId_Status_Type_EndTime",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_EndTime_Status",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_BoatId_Status_StartTime",
                table: "Reservations");

            // Restore old indexes
            migrationBuilder.CreateIndex(
                name: "IX_Reservations_UserId_EndTime",
                table: "Reservations",
                columns: new[] { "UserId", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_UserId_ReservationType",
                table: "Reservations",
                columns: new[] { "UserId", "ReservationType" });
        }
    }
}
