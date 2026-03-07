using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookWorm.Rating.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Unique_Comment_Index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "book_id",
                table: "feedbacks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
            );

            migrationBuilder.CreateIndex(
                name: "ix_feedbacks_book_id_first_name_last_name",
                table: "feedbacks",
                columns: new[] { "book_id", "first_name", "last_name" },
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_feedbacks_book_id_first_name_last_name",
                table: "feedbacks"
            );

            migrationBuilder.DropColumn(name: "book_id", table: "feedbacks");
        }
    }
}
