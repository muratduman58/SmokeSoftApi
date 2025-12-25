using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmokeSoft.Services.ShadowGuard.Migrations
{
    /// <inheritdoc />
    public partial class AddFileHashToScreenCustomization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileHash",
                schema: "shadowguard",
                table: "screen_customizations",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileHash",
                schema: "shadowguard",
                table: "screen_customizations");
        }
    }
}
