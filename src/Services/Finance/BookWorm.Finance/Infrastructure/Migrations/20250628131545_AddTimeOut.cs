using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookWorm.Finance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeOut : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "place_order_timeout_token_id",
                table: "order_state",
                type: "uuid",
                nullable: true
            );

            migrationBuilder.AddColumn<int>(
                name: "timeout_retry_count",
                table: "order_state",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "place_order_timeout_token_id", table: "order_state");

            migrationBuilder.DropColumn(name: "timeout_retry_count", table: "order_state");
        }
    }
}
