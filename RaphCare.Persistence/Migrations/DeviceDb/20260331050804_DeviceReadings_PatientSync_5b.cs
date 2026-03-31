using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaphCare.Persistence.Migrations.DeviceDb
{
    /// <inheritdoc />
    public partial class DeviceReadings_PatientSync_5b : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeviceFirmwares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ReleasedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceFirmwares", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceManufacturers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SupportContact = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceManufacturers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceTypes", x => x.Id);
                });

            var catalogSeedUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var wearableBleTypeId = Guid.Parse("f8c3b2a1-4d5e-4f6a-9b0c-1d2e3f4a5b6c");
            var oemManufacturerId = Guid.Parse("a7b6c5d4-e3f2-4a1b-9c8d-7e6f5a4b3c2d");

            migrationBuilder.InsertData(
                table: "DeviceManufacturers",
                columns: new[] { "Id", "Name", "Country", "SupportContact", "CreatedAt" },
                values: new object[] { oemManufacturerId, "OEM / Fleet", null, null, catalogSeedUtc });

            migrationBuilder.InsertData(
                table: "DeviceTypes",
                columns: new[] { "Id", "Name", "Category", "Description", "CreatedAt" },
                values: new object[] { wearableBleTypeId, "Wearable BLE", "Wearable", "Patient phone gateway (BLE band / watch).", catalogSeedUtc });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeviceTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceManufacturerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceFirmwareId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ActivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsAssigned = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devices_DeviceFirmwares_DeviceFirmwareId",
                        column: x => x.DeviceFirmwareId,
                        principalTable: "DeviceFirmwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Devices_DeviceManufacturers_DeviceManufacturerId",
                        column: x => x.DeviceManufacturerId,
                        principalTable: "DeviceManufacturers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Devices_DeviceTypes_DeviceTypeId",
                        column: x => x.DeviceTypeId,
                        principalTable: "DeviceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeviceAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceAssignments_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeviceReadings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadingType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PrimaryValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    IsFlagged = table.Column<bool>(type: "bit", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Systolic = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Diastolic = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HeartRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RhythmClassification = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    GlucoseLevel = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsFasting = table.Column<bool>(type: "bit", nullable: true),
                    HeartRateReading_HeartRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SpO2 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PulseRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BMI = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceReadings_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAssignments_DeviceId",
                table: "DeviceAssignments",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceReadings_DeviceId",
                table: "DeviceReadings",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceReadings_PatientId",
                table: "DeviceReadings",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceReadings_RecordedAt",
                table: "DeviceReadings",
                column: "RecordedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_ClinicId",
                table: "Devices",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_DeviceFirmwareId",
                table: "Devices",
                column: "DeviceFirmwareId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_DeviceManufacturerId",
                table: "Devices",
                column: "DeviceManufacturerId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_DeviceTypeId",
                table: "Devices",
                column: "DeviceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_SerialNumber",
                table: "Devices",
                column: "SerialNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceAssignments");

            migrationBuilder.DropTable(
                name: "DeviceReadings");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "DeviceFirmwares");

            migrationBuilder.DropTable(
                name: "DeviceManufacturers");

            migrationBuilder.DropTable(
                name: "DeviceTypes");
        }
    }
}
