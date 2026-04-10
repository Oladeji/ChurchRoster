using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ChurchRoster.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProposalModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "roster_proposals",
                columns: table => new
                {
                    proposal_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Processing"),
                    date_range_start = table.Column<DateOnly>(type: "date", nullable: false),
                    date_range_end = table.Column<DateOnly>(type: "date", nullable: false),
                    generated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roster_proposals", x => x.proposal_id);
                    table.ForeignKey(
                        name: "FK_roster_proposals_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "tenant_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_roster_proposals_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "proposal_skip_logs",
                columns: table => new
                {
                    log_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    proposal_id = table.Column<int>(type: "integer", nullable: false),
                    task_id = table.Column<int>(type: "integer", nullable: false),
                    event_date = table.Column<DateOnly>(type: "date", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    logged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proposal_skip_logs", x => x.log_id);
                    table.ForeignKey(
                        name: "FK_proposal_skip_logs_roster_proposals_proposal_id",
                        column: x => x.proposal_id,
                        principalTable: "roster_proposals",
                        principalColumn: "proposal_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "roster_proposal_items",
                columns: table => new
                {
                    item_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    proposal_id = table.Column<int>(type: "integer", nullable: false),
                    task_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    event_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Proposed"),
                    skip_reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roster_proposal_items", x => x.item_id);
                    table.ForeignKey(
                        name: "FK_roster_proposal_items_roster_proposals_proposal_id",
                        column: x => x.proposal_id,
                        principalTable: "roster_proposals",
                        principalColumn: "proposal_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_roster_proposal_items_tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "tasks",
                        principalColumn: "task_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_roster_proposal_items_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_skip_logs_proposal",
                table: "proposal_skip_logs",
                column: "proposal_id");

            migrationBuilder.CreateIndex(
                name: "idx_proposal_items_proposal",
                table: "roster_proposal_items",
                column: "proposal_id");

            migrationBuilder.CreateIndex(
                name: "IX_roster_proposal_items_task_id",
                table: "roster_proposal_items",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_roster_proposal_items_user_id",
                table: "roster_proposal_items",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_roster_proposals_tenant",
                table: "roster_proposals",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "idx_roster_proposals_tenant_status",
                table: "roster_proposals",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_roster_proposals_created_by_user_id",
                table: "roster_proposals",
                column: "created_by_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "proposal_skip_logs");

            migrationBuilder.DropTable(
                name: "roster_proposal_items");

            migrationBuilder.DropTable(
                name: "roster_proposals");
        }
    }
}
