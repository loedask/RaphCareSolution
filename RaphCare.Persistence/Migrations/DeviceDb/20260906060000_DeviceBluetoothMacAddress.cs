using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaphCare.Persistence;

#nullable disable

namespace RaphCare.Persistence.Migrations.DeviceDb;

/// <inheritdoc />
[DbContext(typeof(DeviceDbContext))]
[Migration("20260906060000_DeviceBluetoothMacAddress")]
public class DeviceBluetoothMacAddress : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "BluetoothMacAddress",
            table: "Devices",
            type: "nvarchar(17)",
            maxLength: 17,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Devices_BluetoothMacAddress",
            table: "Devices",
            column: "BluetoothMacAddress",
            unique: true,
            filter: "[BluetoothMacAddress] IS NOT NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Devices_BluetoothMacAddress",
            table: "Devices");

        migrationBuilder.DropColumn(
            name: "BluetoothMacAddress",
            table: "Devices");
    }
}
