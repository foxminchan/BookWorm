using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookWorm.Ordering.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Addorderstatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "update_date",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2024, 7, 23, 16, 30, 35, 114, DateTimeKind.Utc).AddTicks(8086),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2024, 7, 22, 11, 38, 6, 500, DateTimeKind.Utc).AddTicks(186));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 23, 16, 30, 35, 114, DateTimeKind.Utc).AddTicks(7749),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 7, 22, 11, 38, 6, 499, DateTimeKind.Utc).AddTicks(9981));

            migrationBuilder.AddColumn<byte>(
                name: "status",
                table: "orders",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "update_date",
                table: "order_items",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2024, 7, 23, 16, 30, 35, 115, DateTimeKind.Utc).AddTicks(7192),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2024, 7, 22, 11, 38, 6, 500, DateTimeKind.Utc).AddTicks(5546));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "order_items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 23, 16, 30, 35, 115, DateTimeKind.Utc).AddTicks(6869),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 7, 22, 11, 38, 6, 500, DateTimeKind.Utc).AddTicks(5315));

            migrationBuilder.AlterColumn<DateTime>(
                name: "update_date",
                table: "buyers",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2024, 7, 23, 16, 30, 35, 96, DateTimeKind.Utc).AddTicks(9362),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2024, 7, 22, 11, 38, 6, 498, DateTimeKind.Utc).AddTicks(1380));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "buyers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 23, 16, 30, 35, 96, DateTimeKind.Utc).AddTicks(2245),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 7, 22, 11, 38, 6, 498, DateTimeKind.Utc).AddTicks(1138));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "orders");

            migrationBuilder.AlterColumn<DateTime>(
                name: "update_date",
                table: "orders",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2024, 7, 22, 11, 38, 6, 500, DateTimeKind.Utc).AddTicks(186),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2024, 7, 23, 16, 30, 35, 114, DateTimeKind.Utc).AddTicks(8086));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 22, 11, 38, 6, 499, DateTimeKind.Utc).AddTicks(9981),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 7, 23, 16, 30, 35, 114, DateTimeKind.Utc).AddTicks(7749));

            migrationBuilder.AlterColumn<DateTime>(
                name: "update_date",
                table: "order_items",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2024, 7, 22, 11, 38, 6, 500, DateTimeKind.Utc).AddTicks(5546),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2024, 7, 23, 16, 30, 35, 115, DateTimeKind.Utc).AddTicks(7192));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "order_items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 22, 11, 38, 6, 500, DateTimeKind.Utc).AddTicks(5315),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 7, 23, 16, 30, 35, 115, DateTimeKind.Utc).AddTicks(6869));

            migrationBuilder.AlterColumn<DateTime>(
                name: "update_date",
                table: "buyers",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2024, 7, 22, 11, 38, 6, 498, DateTimeKind.Utc).AddTicks(1380),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2024, 7, 23, 16, 30, 35, 96, DateTimeKind.Utc).AddTicks(9362));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "buyers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 22, 11, 38, 6, 498, DateTimeKind.Utc).AddTicks(1138),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 7, 23, 16, 30, 35, 96, DateTimeKind.Utc).AddTicks(2245));
        }
    }
}
