using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MidStateShuttleService.Migrations
{
    /// <inheritdoc />
    public partial class RemovedDriverFromRoute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Routes_Driver_DriverId",
                table: "Routes");

            migrationBuilder.DropIndex(
                name: "IX_Routes_DriverId",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "DriverId",
                table: "Routes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DriverId",
                table: "Routes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Routes_DriverId",
                table: "Routes",
                column: "DriverId");

            migrationBuilder.AddForeignKey(
                name: "FK_Routes_Driver_DriverId",
                table: "Routes",
                column: "DriverId",
                principalTable: "Driver",
                principalColumn: "DriverId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
