using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionIsOutgoing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_outgoing",
                schema: "public",
                table: "transactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE public.transactions SET is_outgoing = true WHERE type = 2;");
            migrationBuilder.Sql(@"
                WITH ranked_transfers AS (
                    SELECT id, ROW_NUMBER() OVER (PARTITION BY transfer_group_id ORDER BY id) as rn
                    FROM public.transactions
                    WHERE type = 3 AND transfer_group_id IS NOT NULL
                )
                UPDATE public.transactions t
                SET is_outgoing = true
                FROM ranked_transfers r
                WHERE t.id = r.id AND r.rn = 1;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_outgoing",
                schema: "public",
                table: "transactions");
        }
    }
}
