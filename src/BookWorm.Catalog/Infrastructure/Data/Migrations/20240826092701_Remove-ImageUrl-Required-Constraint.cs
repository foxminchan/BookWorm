using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BookWorm.Catalog.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveImageUrlRequiredConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "authors",
                keyColumn: "id",
                keyValue: new Guid("0d968c75-092b-46a0-97c2-3f21057966d4"));

            migrationBuilder.DeleteData(
                table: "authors",
                keyColumn: "id",
                keyValue: new Guid("1f7b2ce7-62ad-4b4a-a652-2ce7684dd054"));

            migrationBuilder.DeleteData(
                table: "authors",
                keyColumn: "id",
                keyValue: new Guid("3a50dcb8-1bd6-4990-8598-70f71221349c"));

            migrationBuilder.DeleteData(
                table: "authors",
                keyColumn: "id",
                keyValue: new Guid("4518b238-e9eb-41b9-9eb5-eb7fe51aa507"));

            migrationBuilder.DeleteData(
                table: "authors",
                keyColumn: "id",
                keyValue: new Guid("53fcdefc-16b9-4dd2-982d-4209ed869dae"));

            migrationBuilder.DeleteData(
                table: "authors",
                keyColumn: "id",
                keyValue: new Guid("9fa7ffd2-dba3-4b0a-86cb-ff839d03dcba"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("0781bdfe-06d6-4938-aec9-a17efce07156"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("54689454-6a59-4759-a279-7ba0f50e8c75"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("583e713c-f005-4132-b01f-bc49a16c560d"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("81b38eb5-a7ba-4c5a-b7b7-2ccf60378200"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("a698b5c2-9c13-4ab4-a4a1-5662af1139eb"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("b58e9d7f-b6cc-4817-ab2b-dd33862a9ff7"));

            migrationBuilder.DeleteData(
                table: "publishers",
                keyColumn: "id",
                keyValue: new Guid("167644c7-3871-426a-a7e7-1baf16008b67"));

            migrationBuilder.DeleteData(
                table: "publishers",
                keyColumn: "id",
                keyValue: new Guid("d30e7959-693f-49d9-9f9f-874ad0415c0e"));

            migrationBuilder.DeleteData(
                table: "publishers",
                keyColumn: "id",
                keyValue: new Guid("d423f7b3-2cb5-495a-abd3-3f41a018b00c"));

            migrationBuilder.DeleteData(
                table: "publishers",
                keyColumn: "id",
                keyValue: new Guid("e1a42e46-e715-4bc3-96e8-dfff72f379e5"));

            migrationBuilder.DeleteData(
                table: "publishers",
                keyColumn: "id",
                keyValue: new Guid("e4b29062-ebff-40ef-b792-c2e3c4647a11"));

            migrationBuilder.DeleteData(
                table: "publishers",
                keyColumn: "id",
                keyValue: new Guid("fe787764-3acf-48dc-b69e-e142d19c8f9b"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "update_date",
                table: "publishers",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2024, 8, 26, 9, 27, 0, 702, DateTimeKind.Utc).AddTicks(366),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2024, 7, 16, 10, 57, 42, 213, DateTimeKind.Utc).AddTicks(6174));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "publishers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 8, 26, 9, 27, 0, 701, DateTimeKind.Utc).AddTicks(9973),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 7, 16, 10, 57, 42, 213, DateTimeKind.Utc).AddTicks(5768));

            migrationBuilder.AlterColumn<DateTime>(
                name: "update_date",
                table: "categories",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2024, 8, 26, 9, 27, 0, 700, DateTimeKind.Utc).AddTicks(2275),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2024, 7, 16, 10, 57, 42, 211, DateTimeKind.Utc).AddTicks(3269));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "categories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 8, 26, 9, 27, 0, 700, DateTimeKind.Utc).AddTicks(1848),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 7, 16, 10, 57, 42, 211, DateTimeKind.Utc).AddTicks(2927));

            migrationBuilder.AlterColumn<DateTime>(
                name: "update_date",
                table: "books",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2024, 8, 26, 9, 27, 0, 682, DateTimeKind.Utc).AddTicks(3841),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2024, 7, 16, 10, 57, 42, 195, DateTimeKind.Utc).AddTicks(1577));

            migrationBuilder.AlterColumn<string>(
                name: "image_url",
                table: "books",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "books",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 8, 26, 9, 27, 0, 682, DateTimeKind.Utc).AddTicks(3415),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 7, 16, 10, 57, 42, 195, DateTimeKind.Utc).AddTicks(1119));

            migrationBuilder.AlterColumn<DateTime>(
                name: "update_date",
                table: "authors",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2024, 8, 26, 9, 27, 0, 675, DateTimeKind.Utc).AddTicks(9852),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2024, 7, 16, 10, 57, 42, 189, DateTimeKind.Utc).AddTicks(4479));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "authors",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 8, 26, 9, 27, 0, 675, DateTimeKind.Utc).AddTicks(2108),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 7, 16, 10, 57, 42, 188, DateTimeKind.Utc).AddTicks(4918));

            migrationBuilder.InsertData(
                table: "authors",
                columns: new[] { "id", "name", "version" },
                values: new object[,]
                {
                    { new Guid("5cf81a1f-8dac-4314-b931-15135acd972a"), "Martin Fowler", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("69c788ea-3b24-4aeb-a643-5eef358f69c6"), "Robert C. Martin", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("82f4f535-f175-4404-b3ab-094be1db411b"), "Kent Beck", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("b4cf02dd-6afa-46df-a355-db4b206215f7"), "Don Box", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("c64a2de6-7604-4a4e-8a83-c96624223dad"), "Joshua Bloch", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("dce74f6b-d0ff-4783-b667-e26ac38c581b"), "Eric Evans", new Guid("00000000-0000-0000-0000-000000000000") }
                });

            migrationBuilder.InsertData(
                table: "categories",
                columns: new[] { "id", "name", "version" },
                values: new object[,]
                {
                    { new Guid("02dc54d1-1736-47ee-9568-e6fe9f4ec5d3"), "Personal Development", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("115ebea2-4213-4172-88ed-48caf2adcd6b"), "Light Novel", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("20b91b32-0d04-418b-b4bd-93a0b2978e16"), "Business", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("9a4ab940-1c83-4cc3-ba74-915a8f2471f4"), "Psychology", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("a4d7e88c-8aac-48c7-860d-78dee7985bef"), "Technology", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("fea069eb-07fd-42a5-adc3-0069ed0ad4e1"), "Science", new Guid("00000000-0000-0000-0000-000000000000") }
                });

            migrationBuilder.InsertData(
                table: "publishers",
                columns: new[] { "id", "name", "version" },
                values: new object[,]
                {
                    { new Guid("18c51962-0831-4b2f-b678-bc1a5bd48de5"), "Apress", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("39de270c-48cd-4fe5-96de-124df9ec4b34"), "Packt Publishing", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("6356a093-b89f-462a-bb5f-67f992cde060"), "No Starch Press", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("68b9511a-bc29-486d-892a-49fdcee7c976"), "Manning Publications", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("87ed07db-dbcc-4488-8c92-b29a749c47bb"), "Pragmatic Bookshelf", new Guid("00000000-0000-0000-0000-000000000000") },
                    { new Guid("bde437ec-3888-4779-b3b2-8a16a9b0aaf8"), "O'Reilly Media", new Guid("00000000-0000-0000-0000-000000000000") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "authors",
                keyColumn: "id",
                keyValue: new Guid("5cf81a1f-8dac-4314-b931-15135acd972a"));

            migrationBuilder.DeleteData(
                table: "authors",
                keyColumn: "id",
                keyValue: new Guid("69c788ea-3b24-4aeb-a643-5eef358f69c6"));

            migrationBuilder.DeleteData(
                table: "authors",
                keyColumn: "id",
                keyValue: new Guid("82f4f535-f175-4404-b3ab-094be1db411b"));

            migrationBuilder.DeleteData(
                table: "authors",
                keyColumn: "id",
                keyValue: new Guid("b4cf02dd-6afa-46df-a355-db4b206215f7"));

            migrationBuilder.DeleteData(
                table: "authors",
                keyColumn: "id",
                keyValue: new Guid("c64a2de6-7604-4a4e-8a83-c96624223dad"));

            migrationBuilder.DeleteData(
                table: "authors",
                keyColumn: "id",
                keyValue: new Guid("dce74f6b-d0ff-4783-b667-e26ac38c581b"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("02dc54d1-1736-47ee-9568-e6fe9f4ec5d3"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("115ebea2-4213-4172-88ed-48caf2adcd6b"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("20b91b32-0d04-418b-b4bd-93a0b2978e16"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("9a4ab940-1c83-4cc3-ba74-915a8f2471f4"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("a4d7e88c-8aac-48c7-860d-78dee7985bef"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("fea069eb-07fd-42a5-adc3-0069ed0ad4e1"));

            migrationBuilder.DeleteData(
                table: "publishers",
                keyColumn: "id",
                keyValue: new Guid("18c51962-0831-4b2f-b678-bc1a5bd48de5"));

            migrationBuilder.DeleteData(
                table: "publishers",
                keyColumn: "id",
                keyValue: new Guid("39de270c-48cd-4fe5-96de-124df9ec4b34"));

            migrationBuilder.DeleteData(
                table: "publishers",
                keyColumn: "id",
                keyValue: new Guid("6356a093-b89f-462a-bb5f-67f992cde060"));

            migrationBuilder.DeleteData(
                table: "publishers",
                keyColumn: "id",
                keyValue: new Guid("68b9511a-bc29-486d-892a-49fdcee7c976"));

            migrationBuilder.DeleteData(
                table: "publishers",
                keyColumn: "id",
                keyValue: new Guid("87ed07db-dbcc-4488-8c92-b29a749c47bb"));

            migrationBuilder.DeleteData(
                table: "publishers",
                keyColumn: "id",
                keyValue: new Guid("bde437ec-3888-4779-b3b2-8a16a9b0aaf8"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "update_date",
                table: "publishers",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2024, 7, 16, 10, 57, 42, 213, DateTimeKind.Utc).AddTicks(6174),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2024, 8, 26, 9, 27, 0, 702, DateTimeKind.Utc).AddTicks(366));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "publishers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 16, 10, 57, 42, 213, DateTimeKind.Utc).AddTicks(5768),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 8, 26, 9, 27, 0, 701, DateTimeKind.Utc).AddTicks(9973));

            migrationBuilder.AlterColumn<DateTime>(
                name: "update_date",
                table: "categories",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2024, 7, 16, 10, 57, 42, 211, DateTimeKind.Utc).AddTicks(3269),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2024, 8, 26, 9, 27, 0, 700, DateTimeKind.Utc).AddTicks(2275));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "categories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 16, 10, 57, 42, 211, DateTimeKind.Utc).AddTicks(2927),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 8, 26, 9, 27, 0, 700, DateTimeKind.Utc).AddTicks(1848));

            migrationBuilder.AlterColumn<DateTime>(
                name: "update_date",
                table: "books",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2024, 7, 16, 10, 57, 42, 195, DateTimeKind.Utc).AddTicks(1577),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2024, 8, 26, 9, 27, 0, 682, DateTimeKind.Utc).AddTicks(3841));

            migrationBuilder.AlterColumn<string>(
                name: "image_url",
                table: "books",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "books",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 16, 10, 57, 42, 195, DateTimeKind.Utc).AddTicks(1119),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 8, 26, 9, 27, 0, 682, DateTimeKind.Utc).AddTicks(3415));

            migrationBuilder.AlterColumn<DateTime>(
                name: "update_date",
                table: "authors",
                type: "timestamp with time zone",
                nullable: true,
                defaultValue: new DateTime(2024, 7, 16, 10, 57, 42, 189, DateTimeKind.Utc).AddTicks(4479),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValue: new DateTime(2024, 8, 26, 9, 27, 0, 675, DateTimeKind.Utc).AddTicks(9852));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_date",
                table: "authors",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2024, 7, 16, 10, 57, 42, 188, DateTimeKind.Utc).AddTicks(4918),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2024, 8, 26, 9, 27, 0, 675, DateTimeKind.Utc).AddTicks(2108));

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
        }
    }
}
