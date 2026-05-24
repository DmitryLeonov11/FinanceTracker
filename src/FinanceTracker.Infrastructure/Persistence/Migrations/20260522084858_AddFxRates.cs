using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFxRates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fx_rates",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    rate_to_usd = table.Column<decimal>(type: "numeric(20,10)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fx_rates", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fx_rates_currency_date",
                schema: "public",
                table: "fx_rates",
                columns: new[] { "currency", "date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fx_rates_date",
                schema: "public",
                table: "fx_rates",
                column: "date");

            // Seed initial rates (approximate references; can be updated via FX endpoints later).
            var seedDate = new DateOnly(2026, 1, 1);
            var seedCreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
            migrationBuilder.InsertData(
                schema: "public",
                table: "fx_rates",
                columns: new[] { "id", "date", "currency", "rate_to_usd", "created_at", "updated_at", "version" },
                values: new object[,]
                {
                    { Guid.Parse("11111111-0000-0000-0000-000000000001"), seedDate, "USD", 1.0000000000m, seedCreatedAt, null, 0L },
                    { Guid.Parse("11111111-0000-0000-0000-000000000002"), seedDate, "EUR", 1.0800000000m, seedCreatedAt, null, 0L },
                    { Guid.Parse("11111111-0000-0000-0000-000000000003"), seedDate, "BYN", 0.3060000000m, seedCreatedAt, null, 0L },
                    { Guid.Parse("11111111-0000-0000-0000-000000000004"), seedDate, "RUB", 0.0107000000m, seedCreatedAt, null, 0L }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fx_rates",
                schema: "public");
        }
    }
}
