using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaphCare.Persistence;

#nullable disable

namespace RaphCare.Persistence.Migrations;

[DbContext(typeof(ClinicalDbContext))]
[Migration("20260912120000_AddClinicCommercialPlan")]
public partial class AddClinicCommercialPlan : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CommercialPlan",
            table: "Clinics",
            type: "nvarchar(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "Clinic");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CommercialPlan",
            table: "Clinics");
    }
}
