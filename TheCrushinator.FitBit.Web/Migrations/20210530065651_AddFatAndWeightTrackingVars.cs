using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TheCrushinator.FitBit.Web.Migrations
{
    public partial class AddFatAndWeightTrackingVars : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FitBitUploadDateTimeUtc",
                table: "BeurerWeightEntries");

            migrationBuilder.AddColumn<DateTime>(
                name: "FitBitFatUploadDateTimeUtc",
                table: "BeurerWeightEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FitBitWeightUploadDateTimeUtc",
                table: "BeurerWeightEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FitbitFatLogId",
                table: "BeurerWeightEntries",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FitbitWeightLogId",
                table: "BeurerWeightEntries",
                type: "bigint",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FitBitFatUploadDateTimeUtc",
                table: "BeurerWeightEntries");

            migrationBuilder.DropColumn(
                name: "FitBitWeightUploadDateTimeUtc",
                table: "BeurerWeightEntries");

            migrationBuilder.DropColumn(
                name: "FitbitFatLogId",
                table: "BeurerWeightEntries");

            migrationBuilder.DropColumn(
                name: "FitbitWeightLogId",
                table: "BeurerWeightEntries");

            migrationBuilder.AddColumn<DateTime>(
                name: "FitBitUploadDateTimeUtc",
                table: "BeurerWeightEntries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
