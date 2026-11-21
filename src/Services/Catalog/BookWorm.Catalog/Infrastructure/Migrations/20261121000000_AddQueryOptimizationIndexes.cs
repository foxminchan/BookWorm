using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BookWorm.Catalog.Infrastructure.Migrations;

[DbContext(typeof(CatalogDbContext))]
[Migration("20261121000000_AddQueryOptimizationIndexes")]
public sealed class AddQueryOptimizationIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(name: "ix_books_name", table: "books", column: "name");

        migrationBuilder.CreateIndex(name: "ix_books_status", table: "books", column: "status");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "ix_books_name", table: "books");

        migrationBuilder.DropIndex(name: "ix_books_status", table: "books");
    }
}
