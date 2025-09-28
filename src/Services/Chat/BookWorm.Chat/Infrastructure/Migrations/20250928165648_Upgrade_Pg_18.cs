using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookWorm.Chat.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Upgrade_Pg_18 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Version", table: "Conversations");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Conversations",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuidv7()",
                oldClrType: typeof(Guid),
                oldType: "uuid"
            );

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "Conversations",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "xmin", table: "Conversations");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Conversations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuidv7()"
            );

            migrationBuilder.AddColumn<Guid>(
                name: "Version",
                table: "Conversations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );
        }
    }
}
