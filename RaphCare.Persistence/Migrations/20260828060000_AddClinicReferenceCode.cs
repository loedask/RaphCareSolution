using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaphCare.Persistence;

#nullable disable

namespace RaphCare.Persistence.Migrations;

[DbContext(typeof(ClinicalDbContext))]
[Migration("20260828060000_AddClinicReferenceCode")]
public partial class AddClinicReferenceCode : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Clinics_RegistrationNumber",
            table: "Clinics");

        migrationBuilder.AddColumn<string>(
            name: "ReferenceCode",
            table: "Clinics",
            type: "nvarchar(12)",
            maxLength: 12,
            nullable: false,
            defaultValue: "");

        migrationBuilder.Sql(
            """
            DECLARE @alpha nvarchar(32) = N'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';
            ;WITH numbered AS (
                SELECT Id, ROW_NUMBER() OVER (ORDER BY CreatedAt, Id) AS Seq
                FROM Clinics
            )
            UPDATE c
            SET ReferenceCode = N'RC-'
                + SUBSTRING(@alpha, ((numbered.Seq - 1) / 33554432) % 32 + 1, 1)
                + SUBSTRING(@alpha, ((numbered.Seq - 1) / 1048576) % 32 + 1, 1)
                + SUBSTRING(@alpha, ((numbered.Seq - 1) / 32768) % 32 + 1, 1)
                + SUBSTRING(@alpha, ((numbered.Seq - 1) / 1024) % 32 + 1, 1)
                + SUBSTRING(@alpha, ((numbered.Seq - 1) / 32) % 32 + 1, 1)
                + SUBSTRING(@alpha, (numbered.Seq - 1) % 32 + 1, 1)
            FROM Clinics c
            INNER JOIN numbered ON numbered.Id = c.Id;
            """);

        migrationBuilder.CreateIndex(
            name: "IX_Clinics_ReferenceCode",
            table: "Clinics",
            column: "ReferenceCode",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Clinics_RegistrationNumber",
            table: "Clinics",
            column: "RegistrationNumber");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Clinics_ReferenceCode",
            table: "Clinics");

        migrationBuilder.DropIndex(
            name: "IX_Clinics_RegistrationNumber",
            table: "Clinics");

        migrationBuilder.DropColumn(
            name: "ReferenceCode",
            table: "Clinics");

        migrationBuilder.CreateIndex(
            name: "IX_Clinics_RegistrationNumber",
            table: "Clinics",
            column: "RegistrationNumber",
            unique: true);
    }
}
