using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaphCare.Persistence;

#nullable disable

namespace RaphCare.Persistence.Migrations;

[DbContext(typeof(ClinicalDbContext))]
[Migration("20260620150000_AddClinicStaffInvitation")]
public partial class AddClinicStaffInvitation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ClinicStaffInvitations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ClinicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                InvitedByApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                InvitedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                LastInvitationSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                AcceptedApplicationUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsCancelled = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ClinicStaffInvitations", x => x.Id);
                table.ForeignKey(
                    name: "FK_ClinicStaffInvitations_Clinics_ClinicId",
                    column: x => x.ClinicId,
                    principalTable: "Clinics",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ClinicStaffInvitations_ClinicId",
            table: "ClinicStaffInvitations",
            column: "ClinicId");

        migrationBuilder.CreateIndex(
            name: "IX_ClinicStaffInvitations_ClinicId_Email",
            table: "ClinicStaffInvitations",
            columns: new[] { "ClinicId", "Email" });

        migrationBuilder.CreateIndex(
            name: "IX_ClinicStaffInvitations_IsCancelled",
            table: "ClinicStaffInvitations",
            column: "IsCancelled");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ClinicStaffInvitations");
    }
}
