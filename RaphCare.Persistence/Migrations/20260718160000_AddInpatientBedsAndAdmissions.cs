using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaphCare.Persistence;

#nullable disable

namespace RaphCare.Persistence.Migrations;

[DbContext(typeof(ClinicalDbContext))]
[Migration("20260718160000_AddInpatientBedsAndAdmissions")]
public partial class AddInpatientBedsAndAdmissions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Wards",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FacilityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Wards", x => x.Id);
                table.ForeignKey(
                    name: "FK_Wards_Clinics_ClinicId",
                    column: x => x.ClinicId,
                    principalTable: "Clinics",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Wards_Facility_FacilityId",
                    column: x => x.FacilityId,
                    principalTable: "Facility",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Rooms",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                WardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                RoomType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Rooms", x => x.Id);
                table.ForeignKey(
                    name: "FK_Rooms_Wards_WardId",
                    column: x => x.WardId,
                    principalTable: "Wards",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Beds",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Label = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Beds", x => x.Id);
                table.ForeignKey(
                    name: "FK_Beds_Rooms_RoomId",
                    column: x => x.RoomId,
                    principalTable: "Rooms",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "InpatientAdmissions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                BedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AdmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                DischargedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                AdmittedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_InpatientAdmissions", x => x.Id);
                table.ForeignKey(
                    name: "FK_InpatientAdmissions_Beds_BedId",
                    column: x => x.BedId,
                    principalTable: "Beds",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_InpatientAdmissions_Clinics_ClinicId",
                    column: x => x.ClinicId,
                    principalTable: "Clinics",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_InpatientAdmissions_Patients_PatientId",
                    column: x => x.PatientId,
                    principalTable: "Patients",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "IX_Wards_ClinicId", table: "Wards", column: "ClinicId");
        migrationBuilder.CreateIndex(name: "IX_Wards_FacilityId", table: "Wards", column: "FacilityId");
        migrationBuilder.CreateIndex(name: "IX_Rooms_WardId", table: "Rooms", column: "WardId");
        migrationBuilder.CreateIndex(name: "IX_Beds_RoomId", table: "Beds", column: "RoomId");
        migrationBuilder.CreateIndex(name: "IX_Beds_Status", table: "Beds", column: "Status");
        migrationBuilder.CreateIndex(name: "IX_InpatientAdmissions_BedId", table: "InpatientAdmissions", column: "BedId");
        migrationBuilder.CreateIndex(name: "IX_InpatientAdmissions_ClinicId", table: "InpatientAdmissions", column: "ClinicId");
        migrationBuilder.CreateIndex(name: "IX_InpatientAdmissions_PatientId", table: "InpatientAdmissions", column: "PatientId");
        migrationBuilder.CreateIndex(
            name: "IX_InpatientAdmissions_ClinicId_Status",
            table: "InpatientAdmissions",
            columns: new[] { "ClinicId", "Status" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "InpatientAdmissions");
        migrationBuilder.DropTable(name: "Beds");
        migrationBuilder.DropTable(name: "Rooms");
        migrationBuilder.DropTable(name: "Wards");
    }
}
