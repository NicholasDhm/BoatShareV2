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
            // Use raw SQL to add the column - works on both SQLite and PostgreSQL
            // PostgreSQL uses 'varchar' or 'text', SQLite uses 'TEXT'
            if (migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                migrationBuilder.Sql("ALTER TABLE \"Boats\" ADD COLUMN \"ImageUrl\" varchar(500) NULL;");
            }
            else
            {
                migrationBuilder.Sql("ALTER TABLE \"Boats\" ADD COLUMN \"ImageUrl\" TEXT NULL;");
            }
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
