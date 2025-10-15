using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MidStateShuttleService.Migrations
{
    /// <inheritdoc />
    public partial class newregistrationfields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Registration",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Registration",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<bool>(
                name: "IsAdult",
                table: "Registration",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Registration",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<int>(
                name: "ReturnDropOffLocationId",
                table: "Registration",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReturnPickUpLocationId",
                table: "Registration",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialRequestDescription",
                table: "Registration",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registration_ReturnDropOffLocationId",
                table: "Registration",
                column: "ReturnDropOffLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Registration_ReturnPickUpLocationId",
                table: "Registration",
                column: "ReturnPickUpLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckIn_DropOffLocationId",
                table: "CheckIn",
                column: "DropOffLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_CheckIn_Location_DropOffLocationId",
                table: "CheckIn",
                column: "DropOffLocationId",
                principalTable: "Location",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Registration_ReturnDropOffLocation",
                table: "Registration",
                column: "ReturnDropOffLocationId",
                principalTable: "Location",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Registration_ReturnPickUpLocation",
                table: "Registration",
                column: "ReturnPickUpLocationId",
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
                name: "FK_Registration_ReturnDropOffLocation",
                table: "Registration");

            migrationBuilder.DropForeignKey(
                name: "FK_Registration_ReturnPickUpLocation",
                table: "Registration");

            migrationBuilder.DropIndex(
                name: "IX_Registration_ReturnDropOffLocationId",
                table: "Registration");

            migrationBuilder.DropIndex(
                name: "IX_Registration_ReturnPickUpLocationId",
                table: "Registration");

            migrationBuilder.DropIndex(
                name: "IX_CheckIn_DropOffLocationId",
                table: "CheckIn");

            migrationBuilder.DropColumn(
                name: "ReturnDropOffLocationId",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "ReturnPickUpLocationId",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "SpecialRequestDescription",
                table: "Registration");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Registration",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Registration",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(60)",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<bool>(
                name: "IsAdult",
                table: "Registration",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Registration",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(60)",
                oldMaxLength: 60);
        }
    }
}
