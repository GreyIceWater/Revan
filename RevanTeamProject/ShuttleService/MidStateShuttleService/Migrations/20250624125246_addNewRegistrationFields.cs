using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MidStateShuttleService.Migrations
{
    /// <inheritdoc />
    public partial class addNewRegistrationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdult",
                table: "Registration",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Term",
                table: "Registration",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdult",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "Term",
                table: "Registration");
        }
    }
}
