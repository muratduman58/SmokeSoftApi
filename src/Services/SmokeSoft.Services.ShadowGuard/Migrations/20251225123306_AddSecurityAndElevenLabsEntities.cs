using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmokeSoft.Services.ShadowGuard.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityAndElevenLabsEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasUnlimitedAISlots",
                schema: "shadowguard",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPro",
                schema: "shadowguard",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProExpiresAt",
                schema: "shadowguard",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalAIMinutes",
                schema: "shadowguard",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalAISlots",
                schema: "shadowguard",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsedAIMinutes",
                schema: "shadowguard",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsedAISlots",
                schema: "shadowguard",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CallEndedAt",
                schema: "shadowguard",
                table: "conversations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CallStartedAt",
                schema: "shadowguard",
                table: "conversations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAICall",
                schema: "shadowguard",
                table: "conversations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MinutesUsed",
                schema: "shadowguard",
                table: "conversations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ConversationSessions",
                schema: "shadowguard",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    WebSocketId = table.Column<string>(type: "text", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AudioChunksSent = table.Column<int>(type: "integer", nullable: false),
                    AudioChunksReceived = table.Column<int>(type: "integer", nullable: false),
                    EstimatedCreditsUsed = table.Column<int>(type: "integer", nullable: false),
                    EndReason = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationSessions_conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalSchema: "shadowguard",
                        principalTable: "conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditUsageLogs",
                schema: "shadowguard",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreditsUsed = table.Column<int>(type: "integer", nullable: false),
                    Operation = table.Column<string>(type: "text", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditUsageLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditUsageLogs_conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalSchema: "shadowguard",
                        principalTable: "conversations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CreditUsageLogs_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "shadowguard",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseVerifications",
                schema: "shadowguard",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Platform = table.Column<string>(type: "text", nullable: false),
                    Receipt = table.Column<string>(type: "text", nullable: false),
                    ProductId = table.Column<string>(type: "text", nullable: false),
                    MinutesGranted = table.Column<int>(type: "integer", nullable: false),
                    SlotsGranted = table.Column<int>(type: "integer", nullable: false),
                    UnlimitedSlotsGranted = table.Column<bool>(type: "boolean", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseVerifications_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "shadowguard",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemSafetyConfigs",
                schema: "shadowguard",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ElevenLabsPlanTier = table.Column<string>(type: "text", nullable: false),
                    MonthlyPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    MonthlyCredits = table.Column<int>(type: "integer", nullable: false),
                    CreditsUsed = table.Column<int>(type: "integer", nullable: false),
                    EstimatedMinutes = table.Column<int>(type: "integer", nullable: false),
                    MinutesUsed = table.Column<int>(type: "integer", nullable: false),
                    AbsoluteMaxConcurrentConnections = table.Column<int>(type: "integer", nullable: false),
                    AbsoluteMaxVoiceSlots = table.Column<int>(type: "integer", nullable: false),
                    AbsoluteMaxConversationMinutes = table.Column<int>(type: "integer", nullable: false),
                    AbsoluteMaxDailyMinutesPerUser = table.Column<int>(type: "integer", nullable: false),
                    AbsoluteMaxDailyCredits = table.Column<int>(type: "integer", nullable: false),
                    CreditWarningThreshold = table.Column<decimal>(type: "numeric", nullable: false),
                    CreditDangerThreshold = table.Column<decimal>(type: "numeric", nullable: false),
                    EnableHardLimits = table.Column<bool>(type: "boolean", nullable: false),
                    EnableAutoStop = table.Column<bool>(type: "boolean", nullable: false),
                    EnableDailyLimits = table.Column<bool>(type: "boolean", nullable: false),
                    AlertEmail = table.Column<string>(type: "text", nullable: false),
                    AlertPhone = table.Column<string>(type: "text", nullable: false),
                    SlackWebhookUrl = table.Column<string>(type: "text", nullable: false),
                    PeriodStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AutoRenew = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSafetyConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VoiceSamples",
                schema: "shadowguard",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AIIdentityId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlobUrl = table.Column<string>(type: "text", nullable: false),
                    ParametersJson = table.Column<string>(type: "text", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoiceSamples", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoiceSamples_ai_identities_AIIdentityId",
                        column: x => x.AIIdentityId,
                        principalSchema: "shadowguard",
                        principalTable: "ai_identities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VoiceSlots",
                schema: "shadowguard",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AIIdentityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ElevenLabsVoiceId = table.Column<string>(type: "text", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedFromElevenLabsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoiceSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoiceSlots_ai_identities_AIIdentityId",
                        column: x => x.AIIdentityId,
                        principalSchema: "shadowguard",
                        principalTable: "ai_identities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationSessions_ConversationId",
                schema: "shadowguard",
                table: "ConversationSessions",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditUsageLogs_ConversationId",
                schema: "shadowguard",
                table: "CreditUsageLogs",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditUsageLogs_UserId",
                schema: "shadowguard",
                table: "CreditUsageLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseVerifications_UserId",
                schema: "shadowguard",
                table: "PurchaseVerifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VoiceSamples_AIIdentityId",
                schema: "shadowguard",
                table: "VoiceSamples",
                column: "AIIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_VoiceSlots_AIIdentityId",
                schema: "shadowguard",
                table: "VoiceSlots",
                column: "AIIdentityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversationSessions",
                schema: "shadowguard");

            migrationBuilder.DropTable(
                name: "CreditUsageLogs",
                schema: "shadowguard");

            migrationBuilder.DropTable(
                name: "PurchaseVerifications",
                schema: "shadowguard");

            migrationBuilder.DropTable(
                name: "SystemSafetyConfigs",
                schema: "shadowguard");

            migrationBuilder.DropTable(
                name: "VoiceSamples",
                schema: "shadowguard");

            migrationBuilder.DropTable(
                name: "VoiceSlots",
                schema: "shadowguard");

            migrationBuilder.DropColumn(
                name: "HasUnlimitedAISlots",
                schema: "shadowguard",
                table: "users");

            migrationBuilder.DropColumn(
                name: "IsPro",
                schema: "shadowguard",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ProExpiresAt",
                schema: "shadowguard",
                table: "users");

            migrationBuilder.DropColumn(
                name: "TotalAIMinutes",
                schema: "shadowguard",
                table: "users");

            migrationBuilder.DropColumn(
                name: "TotalAISlots",
                schema: "shadowguard",
                table: "users");

            migrationBuilder.DropColumn(
                name: "UsedAIMinutes",
                schema: "shadowguard",
                table: "users");

            migrationBuilder.DropColumn(
                name: "UsedAISlots",
                schema: "shadowguard",
                table: "users");

            migrationBuilder.DropColumn(
                name: "CallEndedAt",
                schema: "shadowguard",
                table: "conversations");

            migrationBuilder.DropColumn(
                name: "CallStartedAt",
                schema: "shadowguard",
                table: "conversations");

            migrationBuilder.DropColumn(
                name: "IsAICall",
                schema: "shadowguard",
                table: "conversations");

            migrationBuilder.DropColumn(
                name: "MinutesUsed",
                schema: "shadowguard",
                table: "conversations");
        }
    }
}
