using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmokeSoft.Services.ShadowGuard.Migrations
{
    /// <inheritdoc />
    public partial class RefactorUserForAnonymousSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ai_identities_users_UserId",
                schema: "shadowguard",
                table: "ai_identities");

            migrationBuilder.DropForeignKey(
                name: "FK_conversations_users_UserId",
                schema: "shadowguard",
                table: "conversations");

            migrationBuilder.DropForeignKey(
                name: "FK_CreditUsageLogs_users_UserId",
                schema: "shadowguard",
                table: "CreditUsageLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_devices_users_UserId",
                schema: "shadowguard",
                table: "devices");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseVerifications_users_UserId",
                schema: "shadowguard",
                table: "PurchaseVerifications");

            migrationBuilder.DropForeignKey(
                name: "FK_refresh_tokens_users_UserId",
                schema: "shadowguard",
                table: "refresh_tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_screen_customizations_users_UserId",
                schema: "shadowguard",
                table: "screen_customizations");

            migrationBuilder.DropForeignKey(
                name: "FK_user_oauth_providers_users_UserId",
                schema: "shadowguard",
                table: "user_oauth_providers");

            migrationBuilder.DropForeignKey(
                name: "FK_user_profiles_users_UserId",
                schema: "shadowguard",
                table: "user_profiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                schema: "shadowguard",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_Email",
                schema: "shadowguard",
                table: "users");

            migrationBuilder.DropColumn(
                name: "FirstName",
                schema: "shadowguard",
                table: "users");

            migrationBuilder.DropColumn(
                name: "LastName",
                schema: "shadowguard",
                table: "users");

            migrationBuilder.RenameTable(
                name: "users",
                schema: "shadowguard",
                newName: "Users",
                newSchema: "shadowguard");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                schema: "shadowguard",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "shadowguard",
                table: "Users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                schema: "shadowguard",
                table: "Users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                schema: "shadowguard",
                table: "Users",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                schema: "shadowguard",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "\"Email\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ai_identities_Users_UserId",
                schema: "shadowguard",
                table: "ai_identities",
                column: "UserId",
                principalSchema: "shadowguard",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_conversations_Users_UserId",
                schema: "shadowguard",
                table: "conversations",
                column: "UserId",
                principalSchema: "shadowguard",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CreditUsageLogs_Users_UserId",
                schema: "shadowguard",
                table: "CreditUsageLogs",
                column: "UserId",
                principalSchema: "shadowguard",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_devices_Users_UserId",
                schema: "shadowguard",
                table: "devices",
                column: "UserId",
                principalSchema: "shadowguard",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseVerifications_Users_UserId",
                schema: "shadowguard",
                table: "PurchaseVerifications",
                column: "UserId",
                principalSchema: "shadowguard",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_refresh_tokens_Users_UserId",
                schema: "shadowguard",
                table: "refresh_tokens",
                column: "UserId",
                principalSchema: "shadowguard",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_screen_customizations_Users_UserId",
                schema: "shadowguard",
                table: "screen_customizations",
                column: "UserId",
                principalSchema: "shadowguard",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_user_oauth_providers_Users_UserId",
                schema: "shadowguard",
                table: "user_oauth_providers",
                column: "UserId",
                principalSchema: "shadowguard",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_profiles_Users_UserId",
                schema: "shadowguard",
                table: "user_profiles",
                column: "UserId",
                principalSchema: "shadowguard",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ai_identities_Users_UserId",
                schema: "shadowguard",
                table: "ai_identities");

            migrationBuilder.DropForeignKey(
                name: "FK_conversations_Users_UserId",
                schema: "shadowguard",
                table: "conversations");

            migrationBuilder.DropForeignKey(
                name: "FK_CreditUsageLogs_Users_UserId",
                schema: "shadowguard",
                table: "CreditUsageLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_devices_Users_UserId",
                schema: "shadowguard",
                table: "devices");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseVerifications_Users_UserId",
                schema: "shadowguard",
                table: "PurchaseVerifications");

            migrationBuilder.DropForeignKey(
                name: "FK_refresh_tokens_Users_UserId",
                schema: "shadowguard",
                table: "refresh_tokens");

            migrationBuilder.DropForeignKey(
                name: "FK_screen_customizations_Users_UserId",
                schema: "shadowguard",
                table: "screen_customizations");

            migrationBuilder.DropForeignKey(
                name: "FK_user_oauth_providers_Users_UserId",
                schema: "shadowguard",
                table: "user_oauth_providers");

            migrationBuilder.DropForeignKey(
                name: "FK_user_profiles_Users_UserId",
                schema: "shadowguard",
                table: "user_profiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                schema: "shadowguard",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                schema: "shadowguard",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                schema: "shadowguard",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "shadowguard",
                newName: "users",
                newSchema: "shadowguard");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                schema: "shadowguard",
                table: "users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "shadowguard",
                table: "users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                schema: "shadowguard",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                schema: "shadowguard",
                table: "users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                schema: "shadowguard",
                table: "users",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                schema: "shadowguard",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ai_identities_users_UserId",
                schema: "shadowguard",
                table: "ai_identities",
                column: "UserId",
                principalSchema: "shadowguard",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_conversations_users_UserId",
                schema: "shadowguard",
                table: "conversations",
                column: "UserId",
                principalSchema: "shadowguard",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CreditUsageLogs_users_UserId",
                schema: "shadowguard",
                table: "CreditUsageLogs",
                column: "UserId",
                principalSchema: "shadowguard",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_devices_users_UserId",
                schema: "shadowguard",
                table: "devices",
                column: "UserId",
                principalSchema: "shadowguard",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseVerifications_users_UserId",
                schema: "shadowguard",
                table: "PurchaseVerifications",
                column: "UserId",
                principalSchema: "shadowguard",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_refresh_tokens_users_UserId",
                schema: "shadowguard",
                table: "refresh_tokens",
                column: "UserId",
                principalSchema: "shadowguard",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_screen_customizations_users_UserId",
                schema: "shadowguard",
                table: "screen_customizations",
                column: "UserId",
                principalSchema: "shadowguard",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_user_oauth_providers_users_UserId",
                schema: "shadowguard",
                table: "user_oauth_providers",
                column: "UserId",
                principalSchema: "shadowguard",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_profiles_users_UserId",
                schema: "shadowguard",
                table: "user_profiles",
                column: "UserId",
                principalSchema: "shadowguard",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
