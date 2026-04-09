using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MidStateShuttleService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMailItemFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecipientName",
                table: "MailItems");

            migrationBuilder.DropColumn(
                name: "TrackingNumber",
                table: "MailItems");

            migrationBuilder.RenameColumn(
                name: "SenderName",
                table: "MailItems",
                newName: "DriverName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DriverName",
                table: "MailItems",
                newName: "SenderName");

            migrationBuilder.AddColumn<string>(
                name: "RecipientName",
                table: "MailItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TrackingNumber",
                table: "MailItems",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);
        }
    }
}
