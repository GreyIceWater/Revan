using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MidStateShuttleService.Migrations
{
    /// <inheritdoc />
    public partial class RideForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Ride_DropOffLocationID",
                table: "Ride",
                column: "DropOffLocationID");

            migrationBuilder.CreateIndex(
                name: "IX_Ride_PickUpLocationID",
                table: "Ride",
                column: "PickUpLocationID");

            migrationBuilder.AddForeignKey(
                name: "FK_Ride_Location_DropOffLocationID",
                table: "Ride",
                column: "DropOffLocationID",
                principalTable: "Location",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Ride_Location_PickUpLocationID",
                table: "Ride",
                column: "PickUpLocationID",
                principalTable: "Location",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ride_Location_DropOffLocationID",
                table: "Ride");

            migrationBuilder.DropForeignKey(
                name: "FK_Ride_Location_PickUpLocationID",
                table: "Ride");

            migrationBuilder.DropIndex(
                name: "IX_Ride_DropOffLocationID",
                table: "Ride");

            migrationBuilder.DropIndex(
                name: "IX_Ride_PickUpLocationID",
                table: "Ride");
        }
    }
}
