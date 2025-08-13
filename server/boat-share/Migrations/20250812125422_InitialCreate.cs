using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace boat_share.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Boats",
                columns: table => new
                {
                    BoatId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AssignedUsersCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boats", x => x.BoatId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StandardQuota = table.Column<int>(type: "integer", nullable: false),
                    SubstitutionQuota = table.Column<int>(type: "integer", nullable: false),
                    ContingencyQuota = table.Column<int>(type: "integer", nullable: false),
                    BoatId = table.Column<int>(type: "integer", nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Boats_BoatId",
                        column: x => x.BoatId,
                        principalTable: "Boats",
                        principalColumn: "BoatId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    ReservationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    BoatId = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalCost = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReservationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.ReservationId);
                    table.ForeignKey(
                        name: "FK_Reservations_Boats_BoatId",
                        column: x => x.BoatId,
                        principalTable: "Boats",
                        principalColumn: "BoatId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Boats",
                columns: new[] { "BoatId", "AssignedUsersCount", "Capacity", "CreatedAt", "Description", "HourlyRate", "IsActive", "Location", "Name", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 0, 6, new DateTime(2025, 8, 12, 12, 54, 21, 723, DateTimeKind.Utc).AddTicks(6830), "Beautiful 30ft sailing yacht perfect for day trips", 150.00m, true, "Marina Bay", "Ocean Explorer", "Sailing Yacht", new DateTime(2025, 8, 12, 12, 54, 21, 723, DateTimeKind.Utc).AddTicks(6960) },
                    { 2, 0, 4, new DateTime(2025, 8, 12, 12, 54, 21, 723, DateTimeKind.Utc).AddTicks(7070), "Fast motor boat for thrilling water adventures", 200.00m, true, "Harbor Point", "Speed Demon", "Motor Boat", new DateTime(2025, 8, 12, 12, 54, 21, 723, DateTimeKind.Utc).AddTicks(7070) }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "BoatId", "ContingencyQuota", "CreatedAt", "Email", "IsActive", "Name", "PasswordHash", "Role", "StandardQuota", "SubstitutionQuota", "UpdatedAt" },
                values: new object[] { 1, 1, 3, new DateTime(2025, 8, 12, 12, 54, 21, 894, DateTimeKind.Utc).AddTicks(9470), "admin@boatshare.com", true, "Admin User", "$2a$11$CVj7neFuB0D4y4EaF/rBTu6TuVBTUXnpVeU9mexzYEC/vqut3Qh1q", "Admin", 10, 5, new DateTime(2025, 8, 12, 12, 54, 21, 894, DateTimeKind.Utc).AddTicks(9630) });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_BoatId_StartTime_EndTime",
                table: "Reservations",
                columns: new[] { "BoatId", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_UserId",
                table: "Reservations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_BoatId",
                table: "Users",
                column: "BoatId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Boats");
        }
    }
}
