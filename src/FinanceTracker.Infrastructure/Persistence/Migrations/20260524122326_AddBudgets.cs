using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "budgets",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    period = table.Column<int>(type: "integer", nullable: false),
                    limit_amount = table.Column<decimal>(type: "numeric(19,4)", nullable: false),
                    limit_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    rollover = table.Column<bool>(type: "boolean", nullable: false),
                    is_closed = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_budgets", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_budgets_user_id_category_id",
                schema: "public",
                table: "budgets",
                columns: new[] { "user_id", "category_id" });

            migrationBuilder.CreateIndex(
                name: "IX_budgets_user_id_is_closed",
                schema: "public",
                table: "budgets",
                columns: new[] { "user_id", "is_closed" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "budgets",
                schema: "public");
        }
    }
}
