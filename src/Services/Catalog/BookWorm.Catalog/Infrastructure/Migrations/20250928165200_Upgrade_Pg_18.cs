using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookWorm.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Upgrade_Pg_18 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "version", table: "books");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "publishers",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuidv7()",
                oldClrType: typeof(Guid),
                oldType: "uuid"
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "categories",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuidv7()",
                oldClrType: typeof(Guid),
                oldType: "uuid"
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "books",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuidv7()",
                oldClrType: typeof(Guid),
                oldType: "uuid"
            );

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "books",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "book_authors",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuidv7()",
                oldClrType: typeof(Guid),
                oldType: "uuid"
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "authors",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuidv7()",
                oldClrType: typeof(Guid),
                oldType: "uuid"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "xmin", table: "books");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "publishers",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuidv7()"
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "categories",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuidv7()"
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "books",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuidv7()"
            );

            migrationBuilder.AddColumn<Guid>(
                name: "version",
                table: "books",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "book_authors",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuidv7()"
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "authors",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuidv7()"
            );
        }
    }
}
