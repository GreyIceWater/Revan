using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MidStateShuttleService.Migrations
{
    /// <inheritdoc />
    public partial class RoutePropertyOnRide : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Ride_RouteId",
                table: "Ride",
                column: "RouteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ride_Routes_RouteId",
                table: "Ride",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "RouteID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ride_Routes_RouteId",
                table: "Ride");

            migrationBuilder.DropIndex(
                name: "IX_Ride_RouteId",
                table: "Ride");
        }
    }
}
