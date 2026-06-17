using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaphCare.Persistence.Migrations.AIDb
{
    /// <inheritdoc />
    public partial class InitialAI : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DashboardSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SnapshotDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SnapshotJson = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RiskCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskScores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WellnessInsights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InsightType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAIGenerated = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WellnessInsights", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DashboardSnapshots_ClinicId",
                table: "DashboardSnapshots",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardSnapshots_ClinicId_SnapshotDate",
                table: "DashboardSnapshots",
                columns: new[] { "ClinicId", "SnapshotDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DashboardSnapshots_SnapshotDate",
                table: "DashboardSnapshots",
                column: "SnapshotDate");

            migrationBuilder.CreateIndex(
                name: "IX_RiskScores_CalculatedAt",
                table: "RiskScores",
                column: "CalculatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RiskScores_ClinicId",
                table: "RiskScores",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskScores_PatientId",
                table: "RiskScores",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_WellnessInsights_GeneratedAt",
                table: "WellnessInsights",
                column: "GeneratedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WellnessInsights_PatientId",
                table: "WellnessInsights",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DashboardSnapshots");

            migrationBuilder.DropTable(
                name: "RiskScores");

            migrationBuilder.DropTable(
                name: "WellnessInsights");
        }
    }
}
