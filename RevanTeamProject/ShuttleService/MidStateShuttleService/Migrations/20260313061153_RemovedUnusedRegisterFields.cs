using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MidStateShuttleService.Migrations
{
    /// <inheritdoc />
    public partial class RemovedUnusedRegisterFields : Migration
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

            migrationBuilder.DropIndex(
                name: "IX_Registration_FridayDropOffLocationID",
                table: "Registration");

            migrationBuilder.DropIndex(
                name: "IX_Registration_FridayPickUpLocationID",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "CanLeaveTime",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "ContactPreference",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "FirstDayExpectingToRide",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "FridayAgreeTerms",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "FridayCanLeaveTime",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "FridayDropOffLocationID",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "FridayMustArriveTime",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "FridayPickUpLocationID",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "FridayTripType",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "MustArriveTime",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "NeedTransportation",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "ReturnSelectedRouteDetail",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "SelectedDaysOfWeek",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "SelectedRouteDetail",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "SpecialDropOffLocation",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "SpecialPickUpLocation",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "SpecialRequest",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "TripType",
                table: "Registration");

            migrationBuilder.DropColumn(
                name: "WhichFriday",
                table: "Registration");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "CanLeaveTime",
                table: "Registration",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPreference",
                table: "Registration",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "FirstDayExpectingToRide",
                table: "Registration",
                type: "date",
                nullable: true);

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

            migrationBuilder.AddColumn<int>(
                name: "FridayDropOffLocationID",
                table: "Registration",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "FridayMustArriveTime",
                table: "Registration",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FridayPickUpLocationID",
                table: "Registration",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FridayTripType",
                table: "Registration",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Registration",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "MustArriveTime",
                table: "Registration",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NeedTransportation",
                table: "Registration",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnSelectedRouteDetail",
                table: "Registration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedDaysOfWeek",
                table: "Registration",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedRouteDetail",
                table: "Registration",
                type: "nvarchar(max)",
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

            migrationBuilder.AddColumn<bool>(
                name: "SpecialRequest",
                table: "Registration",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TripType",
                table: "Registration",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhichFriday",
                table: "Registration",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registration_FridayDropOffLocationID",
                table: "Registration",
                column: "FridayDropOffLocationID");

            migrationBuilder.CreateIndex(
                name: "IX_Registration_FridayPickUpLocationID",
                table: "Registration",
                column: "FridayPickUpLocationID");

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
        }
    }
}
