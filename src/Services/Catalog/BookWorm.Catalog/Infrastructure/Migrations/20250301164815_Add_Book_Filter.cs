using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookWorm.Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Book_Filter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "last_modified_at",
                table: "books",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2025, 3, 1, 16, 48, 15, 491, DateTimeKind.Utc).AddTicks(
                    6763
                ),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2025, 3, 1, 5, 24, 22, 86, DateTimeKind.Utc).AddTicks(
                    4518
                )
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "books",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 3, 1, 16, 48, 15, 491, DateTimeKind.Utc).AddTicks(
                    4708
                ),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2025, 3, 1, 5, 24, 22, 86, DateTimeKind.Utc).AddTicks(
                    2045
                )
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "last_modified_at",
                table: "books",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2025, 3, 1, 5, 24, 22, 86, DateTimeKind.Utc).AddTicks(
                    4518
                ),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(
                    2025,
                    3,
                    1,
                    16,
                    48,
                    15,
                    491,
                    DateTimeKind.Utc
                ).AddTicks(6763)
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "books",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 3, 1, 5, 24, 22, 86, DateTimeKind.Utc).AddTicks(
                    2045
                ),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(
                    2025,
                    3,
                    1,
                    16,
                    48,
                    15,
                    491,
                    DateTimeKind.Utc
                ).AddTicks(4708)
            );
        }
    }
}
