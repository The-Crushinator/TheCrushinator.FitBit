using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TheCrushinator.FitBit.Web.Migrations
{
    public partial class AddScaleEntryTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BeurerWeightEntries",
                columns: table => new
                {
                    EntryId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImportDateTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecordDateTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WeightKg = table.Column<float>(type: "real", nullable: false),
                    BodyFatPct = table.Column<float>(type: "real", nullable: false),
                    WaterPct = table.Column<float>(type: "real", nullable: false),
                    MusclePct = table.Column<float>(type: "real", nullable: false),
                    BoneMassKg = table.Column<float>(type: "real", nullable: false),
                    BMI = table.Column<float>(type: "real", nullable: false),
                    BMRKCal = table.Column<int>(type: "int", nullable: false),
                    AMRKCal = table.Column<int>(type: "int", nullable: false),
                    FitBitUploadDateTimeUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeurerWeightEntries", x => x.EntryId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeurerWeightEntries");
        }
    }
}
