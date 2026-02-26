using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MidStateShuttleService.Migrations
{
    /// <inheritdoc />
    public partial class RegistrationFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckIn_Location_DropOffLocationId",
                table: "CheckIn");

            migrationBuilder.DropForeignKey(
                name: "FK_CheckIn_Location_LocationId",
                table: "CheckIn");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Registration");

            migrationBuilder.AlterColumn<string>(
                name: "TripType",
                table: "Registration",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AddForeignKey(
                name: "FK_CheckIn_Location_DropOffLocationId",
                table: "CheckIn",
                column: "DropOffLocationId",
                principalTable: "Location",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CheckIn_Location_LocationId",
                table: "CheckIn",
                column: "LocationId",
                principalTable: "Location",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CheckIn_Location_DropOffLocationId",
                table: "CheckIn");

            migrationBuilder.DropForeignKey(
                name: "FK_CheckIn_Location_LocationId",
                table: "CheckIn");

            migrationBuilder.AlterColumn<string>(
                name: "TripType",
                table: "Registration",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Registration",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Registration",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Registration",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Registration",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckIn_Location_DropOffLocationId",
                table: "CheckIn",
                column: "DropOffLocationId",
                principalTable: "Location",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CheckIn_Location_LocationId",
                table: "CheckIn",
                column: "LocationId",
                principalTable: "Location",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
