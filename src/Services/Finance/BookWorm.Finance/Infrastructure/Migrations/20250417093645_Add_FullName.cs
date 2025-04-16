using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookWorm.Finance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_FullName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "full_name",
                table: "order_state",
                type: "text",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "full_name", table: "order_state");
        }
    }
}
