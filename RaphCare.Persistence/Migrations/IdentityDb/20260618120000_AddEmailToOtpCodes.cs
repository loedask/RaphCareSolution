using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaphCare.Persistence.Migrations.IdentityDb;

/// <inheritdoc />
public partial class AddEmailToOtpCodes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Email",
            table: "OtpCodes",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AlterColumn<string>(
            name: "PhoneNumber",
            table: "OtpCodes",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "nvarchar(30)",
            oldMaxLength: 30);

        migrationBuilder.CreateIndex(
            name: "IX_OtpCodes_Email",
            table: "OtpCodes",
            column: "Email");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_OtpCodes_Email",
            table: "OtpCodes");

        migrationBuilder.DropColumn(
            name: "Email",
            table: "OtpCodes");

        migrationBuilder.AlterColumn<string>(
            name: "PhoneNumber",
            table: "OtpCodes",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(30)",
            oldMaxLength: 30,
            oldDefaultValue: "");
    }
}
