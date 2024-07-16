using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BookWorm.Catalog.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initializedatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,")
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "authors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValue: new DateTime(2024, 7, 16, 10, 57, 42, 188, DateTimeKind.Utc).AddTicks(4918)),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValue: new DateTime(2024, 7, 16, 10, 57, 42, 189, DateTimeKind.Utc).AddTicks(4479)),
                    version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_authors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValue: new DateTime(2024, 7, 16, 10, 57, 42, 211, DateTimeKind.Utc).AddTicks(2927)),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValue: new DateTime(2024, 7, 16, 10, 57, 42, 211, DateTimeKind.Utc).AddTicks(3269)),
                    version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "publishers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValue: new DateTime(2024, 7, 16, 10, 57, 42, 213, DateTimeKind.Utc).AddTicks(5768)),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValue: new DateTime(2024, 7, 16, 10, 57, 42, 213, DateTimeKind.Utc).AddTicks(6174)),
                    version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_publishers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "books",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<byte>(type: "smallint", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    average_rating = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    total_reviews = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    publisher_id = table.Column<Guid>(type: "uuid", nullable: true),
                    embedding = table.Column<Vector>(type: "vector(384)", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValue: new DateTime(2024, 7, 16, 10, 57, 42, 195, DateTimeKind.Utc).AddTicks(1119)),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValue: new DateTime(2024, 7, 16, 10, 57, 42, 195, DateTimeKind.Utc).AddTicks(1577)),
                    version = table.Column<Guid>(type: "uuid", nullable: false),
                    price = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_books", x => x.id);
                    table.ForeignKey(
                        name: "fk_books_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_books_publishers_publisher_id",
                        column: x => x.publisher_id,
                        principalTable: "publishers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "book_authors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    book_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_book_authors", x => x.id);
                    table.ForeignKey(
                        name: "fk_book_authors_authors_author_id",
                        column: x => x.author_id,
                        principalTable: "authors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_book_authors_books_book_id",
                        column: x => x.book_id,
                        principalTable: "books",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "authors",
                columns: new[] { "id", "name", "version" },
                values: new object[,]
                {
                    { new Guid("0d968c75-092b-46a0-97c2-3f21057966d4"), "Don Box", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("1f7b2ce7-62ad-4b4a-a652-2ce7684dd054"), "Joshua Bloch", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("3a50dcb8-1bd6-4990-8598-70f71221349c"), "Eric Evans", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("4518b238-e9eb-41b9-9eb5-eb7fe51aa507"), "Kent Beck", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("53fcdefc-16b9-4dd2-982d-4209ed869dae"), "Robert C. Martin", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("9fa7ffd2-dba3-4b0a-86cb-ff839d03dcba"), "Martin Fowler", new Guid("00000000-0000-0000-0000-000000000000") }
                });

            migrationBuilder.InsertData(
                table: "categories",
                columns: new[] { "id", "name", "version" },
                values: new object[,]
                {
                    { new Guid("0781bdfe-06d6-4938-aec9-a17efce07156"), "Science", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("54689454-6a59-4759-a279-7ba0f50e8c75"), "Technology", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("583e713c-f005-4132-b01f-bc49a16c560d"), "Personal Development", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("81b38eb5-a7ba-4c5a-b7b7-2ccf60378200"), "Business", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("a698b5c2-9c13-4ab4-a4a1-5662af1139eb"), "Light Novel", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("b58e9d7f-b6cc-4817-ab2b-dd33862a9ff7"), "Psychology", new Guid("00000000-0000-0000-0000-000000000000") }
                });

            migrationBuilder.InsertData(
                table: "publishers",
                columns: new[] { "id", "name", "version" },
                values: new object[,]
                {
                    { new Guid("167644c7-3871-426a-a7e7-1baf16008b67"), "Packt Publishing", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("d30e7959-693f-49d9-9f9f-874ad0415c0e"), "O'Reilly Media", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("d423f7b3-2cb5-495a-abd3-3f41a018b00c"), "Manning Publications", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("e1a42e46-e715-4bc3-96e8-dfff72f379e5"), "No Starch Press", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("e4b29062-ebff-40ef-b792-c2e3c4647a11"), "Pragmatic Bookshelf", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("fe787764-3acf-48dc-b69e-e142d19c8f9b"), "Apress", new Guid("00000000-0000-0000-0000-000000000000") }
                });

            migrationBuilder.CreateIndex(
                name: "ix_book_authors_author_id",
                table: "book_authors",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "ix_book_authors_book_id",
                table: "book_authors",
                column: "book_id");

            migrationBuilder.CreateIndex(
                name: "ix_books_category_id",
                table: "books",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_books_publisher_id",
                table: "books",
                column: "publisher_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_name",
                table: "categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_publishers_name",
                table: "publishers",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "book_authors");

            migrationBuilder.DropTable(
                name: "authors");

            migrationBuilder.DropTable(
                name: "books");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "publishers");
        }
    }
}
