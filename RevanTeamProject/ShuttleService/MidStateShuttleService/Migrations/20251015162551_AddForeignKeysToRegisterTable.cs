using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MidStateShuttleService.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKeysToRegisterTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Registration_FridayDropOffLocation",
                table: "Registration");

            migrationBuilder.DropForeignKey(
                name: "FK_Registration_FridayPickUpLocation",
                table: "Registration");

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

            migrationBuilder.DropColumn(
                name: "FridayAgreeTerms",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "FridayCanLeaveTime",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "FridayMustArriveTime",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "FridayTripType",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "SpecialDropOffLocation",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "SpecialPickUpLocation",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "WhichFriday",
                table: "Registration");

            migrationBuilder.RenameColumn(
                name: "FridayPickUpLocationID",
                table: "Registration",
                newName: "RouteId");

            migrationBuilder.RenameColumn(
                name: "FridayDropOffLocationID",
                table: "Registration",
                newName: "ReturnRouteId");

            migrationBuilder.RenameIndex(
                name: "IX_Registration_FridayPickUpLocationID",
                table: "Registration",
                newName: "IX_Registration_RouteId");

            migrationBuilder.RenameIndex(
                name: "IX_Registration_FridayDropOffLocationID",
                table: "Registration",
                newName: "IX_Registration_ReturnRouteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Registration_ReturnRoute",
                table: "Registration",
                column: "ReturnRouteId",
                principalTable: "Routes",
                principalColumn: "RouteID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Registration_Route",
                table: "Registration",
                column: "RouteId",
                principalTable: "Routes",
                principalColumn: "RouteID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Registration_ReturnRoute",
                table: "Registration");

            migrationBuilder.DropForeignKey(
                name: "FK_Registration_Route",
                table: "Registration");

            migrationBuilder.RenameColumn(
                name: "RouteId",
                table: "Registration",
                newName: "FridayPickUpLocationID");

            migrationBuilder.RenameColumn(
                name: "ReturnRouteId",
                table: "Registration",
                newName: "FridayDropOffLocationID");

            migrationBuilder.RenameIndex(
                name: "IX_Registration_RouteId",
                table: "Registration",
                newName: "IX_Registration_FridayPickUpLocationID");

            migrationBuilder.RenameIndex(
                name: "IX_Registration_ReturnRouteId",
                table: "Registration",
                newName: "IX_Registration_FridayDropOffLocationID");

            migrationBuilder.AddColumn<bool>(
                name: "FridayAgreeTerms",
                table: "Registration",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "FridayCanLeaveTime",
                table: "Registration",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "FridayMustArriveTime",
                table: "Registration",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FridayTripType",
                table: "Registration",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialDropOffLocation",
                table: "Registration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialPickUpLocation",
                table: "Registration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhichFriday",
                table: "Registration",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registration_ReturnDropOffLocationId",
                table: "Registration",
                column: "ReturnDropOffLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Registration_ReturnPickUpLocationId",
                table: "Registration",
                column: "ReturnPickUpLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Registration_FridayDropOffLocation",
                table: "Registration",
                column: "FridayDropOffLocationID",
                principalTable: "Location",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Registration_FridayPickUpLocation",
                table: "Registration",
                column: "FridayPickUpLocationID",
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
    }
}
