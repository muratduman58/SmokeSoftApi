using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmokeSoft.Services.ShadowGuard.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalizationStrings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "localization_strings",
                schema: "shadowguard",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_localization_strings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_localization_strings_Category",
                schema: "shadowguard",
                table: "localization_strings",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_localization_strings_Key_LanguageCode",
                schema: "shadowguard",
                table: "localization_strings",
                columns: new[] { "Key", "LanguageCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "localization_strings",
                schema: "shadowguard");
        }
    }
}
