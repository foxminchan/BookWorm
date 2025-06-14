using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookWorm.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UnfreezeTheTimeStamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "last_modified_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "NOW() AT TIME ZONE 'UTC'",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2025, 3, 4, 8, 1, 43, 54, DateTimeKind.Utc).AddTicks(
                    8168
                )
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW() AT TIME ZONE 'UTC'",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2025, 3, 4, 8, 1, 43, 53, DateTimeKind.Utc).AddTicks(
                    7257
                )
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "last_modified_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2025, 3, 4, 8, 1, 43, 54, DateTimeKind.Utc).AddTicks(
                    8168
                ),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "NOW() AT TIME ZONE 'UTC'"
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 3, 4, 8, 1, 43, 53, DateTimeKind.Utc).AddTicks(
                    7257
                ),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW() AT TIME ZONE 'UTC'"
            );
        }
    }
}
