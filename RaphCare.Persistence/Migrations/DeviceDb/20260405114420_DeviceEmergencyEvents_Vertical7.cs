using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaphCare.Persistence.Migrations.DeviceDb
{
    /// <inheritdoc />
    public partial class DeviceEmergencyEvents_Vertical7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceEmergencyEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    HorizontalAccuracyMeters = table.Column<double>(type: "float", nullable: true),
                    ExternalCorrelationId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CaregiversNotified = table.Column<bool>(type: "bit", nullable: false),
                    CaregiversNotifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CaregiverNotificationSummary = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceEmergencyEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceEmergencyEvents_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEmergencyEvents_ClinicId",
                table: "DeviceEmergencyEvents",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEmergencyEvents_DeviceId",
                table: "DeviceEmergencyEvents",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEmergencyEvents_DeviceId_ExternalCorrelationId",
                table: "DeviceEmergencyEvents",
                columns: new[] { "DeviceId", "ExternalCorrelationId" },
                unique: true,
                filter: "[ExternalCorrelationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEmergencyEvents_OccurredAtUtc",
                table: "DeviceEmergencyEvents",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEmergencyEvents_PatientId",
                table: "DeviceEmergencyEvents",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceEmergencyEvents");
        }
    }
}
