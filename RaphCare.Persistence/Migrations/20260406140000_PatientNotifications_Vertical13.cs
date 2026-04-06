using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaphCare.Persistence;

#nullable disable

namespace RaphCare.Persistence.Migrations;

[DbContext(typeof(ClinicalDbContext))]
[Migration("20260406140000_PatientNotifications_Vertical13")]
public partial class PatientNotifications_Vertical13 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PatientInAppNotifications",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Body = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                Type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                IsRead = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PatientInAppNotifications", x => x.Id);
                table.ForeignKey(
                    name: "FK_PatientInAppNotifications_Patients_PatientId",
                    column: x => x.PatientId,
                    principalTable: "Patients",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "PatientPushDevices",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DeviceToken = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                Platform = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PatientPushDevices", x => x.Id);
                table.ForeignKey(
                    name: "FK_PatientPushDevices_Patients_PatientId",
                    column: x => x.PatientId,
                    principalTable: "Patients",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PatientInAppNotifications_PatientId",
            table: "PatientInAppNotifications",
            column: "PatientId");

        migrationBuilder.CreateIndex(
            name: "IX_PatientInAppNotifications_PatientId_CreatedAt",
            table: "PatientInAppNotifications",
            columns: new[] { "PatientId", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_PatientPushDevices_PatientId",
            table: "PatientPushDevices",
            column: "PatientId");

        migrationBuilder.CreateIndex(
            name: "IX_PatientPushDevices_PatientId_DeviceToken",
            table: "PatientPushDevices",
            columns: new[] { "PatientId", "DeviceToken" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PatientPushDevices");
        migrationBuilder.DropTable(name: "PatientInAppNotifications");
    }
}
