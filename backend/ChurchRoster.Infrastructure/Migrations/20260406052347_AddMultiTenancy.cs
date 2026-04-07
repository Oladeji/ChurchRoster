using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ChurchRoster.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_users_email",
                table: "users");

            migrationBuilder.DropIndex(
                name: "idx_skills_name",
                table: "skills");

            migrationBuilder.DropIndex(
                name: "idx_invitations_email",
                table: "invitations");

            migrationBuilder.AddColumn<int>(
                name: "tenant_id",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "tenant_id",
                table: "user_skills",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "tenant_id",
                table: "tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "tenant_id",
                table: "skills",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "tenant_id",
                table: "invitations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "tenant_id",
                table: "assignments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    tenant_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    contact_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    subscription_end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.tenant_id);
                });

            migrationBuilder.UpdateData(
                table: "skills",
                keyColumn: "skill_id",
                keyValue: 1,
                column: "tenant_id",
                value: 1);

            migrationBuilder.UpdateData(
                table: "skills",
                keyColumn: "skill_id",
                keyValue: 2,
                column: "tenant_id",
                value: 1);

            migrationBuilder.UpdateData(
                table: "skills",
                keyColumn: "skill_id",
                keyValue: 3,
                column: "tenant_id",
                value: 1);

            migrationBuilder.UpdateData(
                table: "skills",
                keyColumn: "skill_id",
                keyValue: 4,
                column: "tenant_id",
                value: 1);

            migrationBuilder.UpdateData(
                table: "skills",
                keyColumn: "skill_id",
                keyValue: 5,
                column: "tenant_id",
                value: 1);

            migrationBuilder.UpdateData(
                table: "skills",
                keyColumn: "skill_id",
                keyValue: 6,
                column: "tenant_id",
                value: 1);

            migrationBuilder.UpdateData(
                table: "skills",
                keyColumn: "skill_id",
                keyValue: 7,
                column: "tenant_id",
                value: 1);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: 1,
                column: "tenant_id",
                value: 1);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: 2,
                column: "tenant_id",
                value: 1);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: 3,
                column: "tenant_id",
                value: 1);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: 4,
                column: "tenant_id",
                value: 1);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: 5,
                column: "tenant_id",
                value: 1);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: 6,
                column: "tenant_id",
                value: 1);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: 7,
                column: "tenant_id",
                value: 1);

            migrationBuilder.UpdateData(
                table: "tasks",
                keyColumn: "task_id",
                keyValue: 8,
                column: "tenant_id",
                value: 1);

            migrationBuilder.InsertData(
                table: "tenants",
                columns: new[] { "tenant_id", "contact_email", "created_at", "is_active", "name", "slug", "subscription_end_date" },
                values: new object[] { 1, "admin@church.com", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Default Church", "default-church", new DateTime(2036, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "user_id",
                keyValue: 1,
                column: "tenant_id",
                value: 1);

            migrationBuilder.CreateIndex(
                name: "idx_users_tenant_email",
                table: "users",
                columns: new[] { "tenant_id", "email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_skills_tenant_id",
                table: "user_skills",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_tasks_tenant_id",
                table: "tasks",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "idx_skills_tenant_name",
                table: "skills",
                columns: new[] { "tenant_id", "skill_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_invitations_tenant_email",
                table: "invitations",
                columns: new[] { "tenant_id", "email" });

            migrationBuilder.CreateIndex(
                name: "IX_assignments_tenant_id",
                table: "assignments",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "idx_tenants_slug",
                table: "tenants",
                column: "slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_assignments_tenants_tenant_id",
                table: "assignments",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "tenant_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_invitations_tenants_tenant_id",
                table: "invitations",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "tenant_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_skills_tenants_tenant_id",
                table: "skills",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "tenant_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_tasks_tenants_tenant_id",
                table: "tasks",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "tenant_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_user_skills_tenants_tenant_id",
                table: "user_skills",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "tenant_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_users_tenants_tenant_id",
                table: "users",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "tenant_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_assignments_tenants_tenant_id",
                table: "assignments");

            migrationBuilder.DropForeignKey(
                name: "FK_invitations_tenants_tenant_id",
                table: "invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_skills_tenants_tenant_id",
                table: "skills");

            migrationBuilder.DropForeignKey(
                name: "FK_tasks_tenants_tenant_id",
                table: "tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_user_skills_tenants_tenant_id",
                table: "user_skills");

            migrationBuilder.DropForeignKey(
                name: "FK_users_tenants_tenant_id",
                table: "users");

            migrationBuilder.DropTable(
                name: "tenants");

            migrationBuilder.DropIndex(
                name: "idx_users_tenant_email",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_user_skills_tenant_id",
                table: "user_skills");

            migrationBuilder.DropIndex(
                name: "IX_tasks_tenant_id",
                table: "tasks");

            migrationBuilder.DropIndex(
                name: "idx_skills_tenant_name",
                table: "skills");

            migrationBuilder.DropIndex(
                name: "idx_invitations_tenant_email",
                table: "invitations");

            migrationBuilder.DropIndex(
                name: "IX_assignments_tenant_id",
                table: "assignments");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "user_skills");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "skills");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "invitations");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "assignments");

            migrationBuilder.CreateIndex(
                name: "idx_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_skills_name",
                table: "skills",
                column: "skill_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_invitations_email",
                table: "invitations",
                column: "email");
        }
    }
}
