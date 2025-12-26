using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmokeSoft.Services.ShadowGuard.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMaintenanceMode",
                schema: "shadowguard",
                table: "SystemSafetyConfigs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MaintenanceMessage",
                schema: "shadowguard",
                table: "SystemSafetyConfigs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MaintenanceStartedAt",
                schema: "shadowguard",
                table: "SystemSafetyConfigs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaintenanceStartedBy",
                schema: "shadowguard",
                table: "SystemSafetyConfigs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMaintenanceMode",
                schema: "shadowguard",
                table: "SystemSafetyConfigs");

            migrationBuilder.DropColumn(
                name: "MaintenanceMessage",
                schema: "shadowguard",
                table: "SystemSafetyConfigs");

            migrationBuilder.DropColumn(
                name: "MaintenanceStartedAt",
                schema: "shadowguard",
                table: "SystemSafetyConfigs");

            migrationBuilder.DropColumn(
                name: "MaintenanceStartedBy",
                schema: "shadowguard",
                table: "SystemSafetyConfigs");
        }
    }
}
