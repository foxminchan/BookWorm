using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace BookWorm.Catalog.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initiallizedatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,")
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "authors",
                columns: table => new
                {
                    id = table.Column<Guid>(
                        type: "uuid",
                        nullable: false,
                        defaultValueSql: "uuid_generate_v4()"
                    ),
                    name = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    created_date = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValue: new DateTime(
                            2024,
                            9,
                            13,
                            17,
                            14,
                            29,
                            326,
                            DateTimeKind.Utc
                        ).AddTicks(1649)
                    ),
                    update_date = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true,
                        defaultValue: new DateTime(
                            2024,
                            9,
                            13,
                            17,
                            14,
                            29,
                            326,
                            DateTimeKind.Utc
                        ).AddTicks(1945)
                    ),
                    version = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_authors", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(
                        type: "uuid",
                        nullable: false,
                        defaultValueSql: "uuid_generate_v4()"
                    ),
                    name = table.Column<string>(
                        type: "character varying(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    created_date = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValue: new DateTime(
                            2024,
                            9,
                            13,
                            17,
                            14,
                            29,
                            328,
                            DateTimeKind.Utc
                        ).AddTicks(2869)
                    ),
                    update_date = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true,
                        defaultValue: new DateTime(
                            2024,
                            9,
                            13,
                            17,
                            14,
                            29,
                            328,
                            DateTimeKind.Utc
                        ).AddTicks(3071)
                    ),
                    version = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "publishers",
                columns: table => new
                {
                    id = table.Column<Guid>(
                        type: "uuid",
                        nullable: false,
                        defaultValueSql: "uuid_generate_v4()"
                    ),
                    name = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    created_date = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValue: new DateTime(
                            2024,
                            9,
                            13,
                            17,
                            14,
                            29,
                            328,
                            DateTimeKind.Utc
                        ).AddTicks(5073)
                    ),
                    update_date = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true,
                        defaultValue: new DateTime(
                            2024,
                            9,
                            13,
                            17,
                            14,
                            29,
                            328,
                            DateTimeKind.Utc
                        ).AddTicks(5268)
                    ),
                    version = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_publishers", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "books",
                columns: table => new
                {
                    id = table.Column<Guid>(
                        type: "uuid",
                        nullable: false,
                        defaultValueSql: "uuid_generate_v4()"
                    ),
                    name = table.Column<string>(
                        type: "character varying(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    description = table.Column<string>(
                        type: "character varying(500)",
                        maxLength: 500,
                        nullable: false
                    ),
                    image_url = table.Column<string>(
                        type: "character varying(500)",
                        maxLength: 500,
                        nullable: true
                    ),
                    status = table.Column<byte>(type: "smallint", nullable: false),
                    average_rating = table.Column<double>(
                        type: "double precision",
                        nullable: false,
                        defaultValue: 0.0
                    ),
                    total_reviews = table.Column<int>(
                        type: "integer",
                        nullable: false,
                        defaultValue: 0
                    ),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    publisher_id = table.Column<Guid>(type: "uuid", nullable: true),
                    embedding = table.Column<Vector>(type: "vector(384)", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_date = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValue: new DateTime(
                            2024,
                            9,
                            13,
                            17,
                            14,
                            29,
                            326,
                            DateTimeKind.Utc
                        ).AddTicks(5074)
                    ),
                    update_date = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true,
                        defaultValue: new DateTime(
                            2024,
                            9,
                            13,
                            17,
                            14,
                            29,
                            326,
                            DateTimeKind.Utc
                        ).AddTicks(5792)
                    ),
                    version = table.Column<Guid>(type: "uuid", nullable: false),
                    price = table.Column<string>(type: "jsonb", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_books", x => x.id);
                    table.ForeignKey(
                        name: "fk_books_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                    table.ForeignKey(
                        name: "fk_books_publishers_publisher_id",
                        column: x => x.publisher_id,
                        principalTable: "publishers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "book_authors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    book_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_date = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    update_date = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    version = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_book_authors", x => x.id);
                    table.ForeignKey(
                        name: "fk_book_authors_authors_author_id",
                        column: x => x.author_id,
                        principalTable: "authors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "fk_book_authors_books_book_id",
                        column: x => x.book_id,
                        principalTable: "books",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_book_authors_author_id",
                table: "book_authors",
                column: "author_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_book_authors_book_id",
                table: "book_authors",
                column: "book_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_books_category_id",
                table: "books",
                column: "category_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_books_publisher_id",
                table: "books",
                column: "publisher_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_categories_name",
                table: "categories",
                column: "name",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_publishers_name",
                table: "publishers",
                column: "name",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "book_authors");

            migrationBuilder.DropTable(name: "authors");

            migrationBuilder.DropTable(name: "books");

            migrationBuilder.DropTable(name: "categories");

            migrationBuilder.DropTable(name: "publishers");
        }
    }
}
