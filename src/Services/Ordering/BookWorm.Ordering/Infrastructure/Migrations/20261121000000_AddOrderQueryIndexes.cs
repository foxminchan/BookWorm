using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BookWorm.Ordering.Infrastructure.Migrations;

[DbContext(typeof(OrderingDbContext))]
[Migration("20261121000000_AddOrderQueryIndexes")]
public sealed class AddOrderQueryIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "ix_orders_buyer_id_created_at",
            table: "orders",
            columns: ["buyer_id", "created_at"]
        );

        migrationBuilder.CreateIndex(
            name: "ix_orders_status_created_at",
            table: "orders",
            columns: ["status", "created_at"]
        );

        migrationBuilder.CreateIndex(
            name: "ix_orders_created_at",
            table: "orders",
            column: "created_at"
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "ix_orders_buyer_id_created_at", table: "orders");

        migrationBuilder.DropIndex(name: "ix_orders_status_created_at", table: "orders");

        migrationBuilder.DropIndex(name: "ix_orders_created_at", table: "orders");
    }
}
