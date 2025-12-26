using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmokeSoft.Services.Admin.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultUuidGeneration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add default UUID generation for admin_users table
            migrationBuilder.Sql(@"
                ALTER TABLE admin.admin_users 
                ALTER COLUMN ""Id"" SET DEFAULT gen_random_uuid();
            ");

            // Add default UUID generation for audit_logs table
            migrationBuilder.Sql(@"
                ALTER TABLE admin.audit_logs 
                ALTER COLUMN ""Id"" SET DEFAULT gen_random_uuid();
            ");

            // Add default UUID generation for system_metrics table
            migrationBuilder.Sql(@"
                ALTER TABLE admin.system_metrics 
                ALTER COLUMN ""Id"" SET DEFAULT gen_random_uuid();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove default UUID generation
            migrationBuilder.Sql(@"
                ALTER TABLE admin.admin_users 
                ALTER COLUMN ""Id"" DROP DEFAULT;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE admin.audit_logs 
                ALTER COLUMN ""Id"" DROP DEFAULT;
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE admin.system_metrics 
                ALTER COLUMN ""Id"" DROP DEFAULT;
            ");
        }
    }
}
