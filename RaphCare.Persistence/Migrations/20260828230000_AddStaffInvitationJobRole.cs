using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaphCare.Persistence;

#nullable disable

namespace RaphCare.Persistence.Migrations;

[DbContext(typeof(ClinicalDbContext))]
[Migration("20260828230000_AddStaffInvitationJobRole")]
public partial class AddStaffInvitationJobRole : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "JobRole",
            table: "ClinicStaffInvitations",
            type: "nvarchar(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "Clinician");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "JobRole", table: "ClinicStaffInvitations");
    }
}
