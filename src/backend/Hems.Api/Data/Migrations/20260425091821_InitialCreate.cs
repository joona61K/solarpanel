using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hems.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutomationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TriggerReason = table.Column<string>(type: "TEXT", nullable: false),
                    DeviceName = table.Column<string>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    Success = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutomationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EnergyReadings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProductionWatts = table.Column<double>(type: "REAL", nullable: false),
                    ConsumptionWatts = table.Column<double>(type: "REAL", nullable: false),
                    GridFeedInWatts = table.Column<double>(type: "REAL", nullable: false),
                    BatteryChargePercent = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnergyReadings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpotPrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HourUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: false),
                    PricePerKwh = table.Column<double>(type: "REAL", nullable: false),
                    Area = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpotPrices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WeatherForecasts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ForecastTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TemperatureCelsius = table.Column<double>(type: "REAL", nullable: false),
                    CloudCoverPercent = table.Column<double>(type: "REAL", nullable: false),
                    SolarRadiationWm2 = table.Column<double>(type: "REAL", nullable: false),
                    PrecipitationMm = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherForecasts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutomationLogs_Timestamp",
                table: "AutomationLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_EnergyReadings_Timestamp",
                table: "EnergyReadings",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_SpotPrices_HourUtc_Area",
                table: "SpotPrices",
                columns: new[] { "HourUtc", "Area" });

            migrationBuilder.CreateIndex(
                name: "IX_WeatherForecasts_ForecastTime",
                table: "WeatherForecasts",
                column: "ForecastTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutomationLogs");

            migrationBuilder.DropTable(
                name: "EnergyReadings");

            migrationBuilder.DropTable(
                name: "SpotPrices");

            migrationBuilder.DropTable(
                name: "WeatherForecasts");
        }
    }
}
