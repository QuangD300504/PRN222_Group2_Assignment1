using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN222_Group2_Assignment1.Migrations
{
    /// <inheritdoc />
    public partial class AddEmbeddingVectorJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmbeddingVectorJson",
                table: "DocumentChunks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EmbeddingVectorJson" },
                values: new object[] { new DateTime(2026, 8, 20, 8, 26, 41, 291, DateTimeKind.Utc).AddTicks(4900), null });

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "EmbeddingVectorJson" },
                values: new object[] { new DateTime(2026, 8, 20, 8, 26, 41, 291, DateTimeKind.Utc).AddTicks(4905), null });

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "EmbeddingVectorJson" },
                values: new object[] { new DateTime(2026, 8, 20, 8, 26, 41, 291, DateTimeKind.Utc).AddTicks(4907), null });

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "EmbeddingVectorJson" },
                values: new object[] { new DateTime(2026, 8, 20, 8, 26, 41, 291, DateTimeKind.Utc).AddTicks(4908), null });

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "EmbeddingVectorJson" },
                values: new object[] { new DateTime(2026, 8, 20, 8, 26, 41, 291, DateTimeKind.Utc).AddTicks(4909), null });

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "EmbeddingVectorJson" },
                values: new object[] { new DateTime(2026, 8, 20, 8, 26, 41, 291, DateTimeKind.Utc).AddTicks(4910), null });

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "EmbeddingVectorJson" },
                values: new object[] { new DateTime(2026, 8, 20, 8, 26, 41, 291, DateTimeKind.Utc).AddTicks(4912), null });

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "EmbeddingVectorJson" },
                values: new object[] { new DateTime(2026, 8, 20, 8, 26, 41, 291, DateTimeKind.Utc).AddTicks(4913), null });

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "CreatedAt", "EmbeddingVectorJson" },
                values: new object[] { new DateTime(2026, 8, 20, 8, 26, 41, 291, DateTimeKind.Utc).AddTicks(4914), null });

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "CreatedAt", "EmbeddingVectorJson" },
                values: new object[] { new DateTime(2026, 8, 20, 8, 26, 41, 291, DateTimeKind.Utc).AddTicks(4915), null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmbeddingVectorJson",
                table: "DocumentChunks");

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 13, 8, 55, 816, DateTimeKind.Utc).AddTicks(9766));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 13, 8, 55, 816, DateTimeKind.Utc).AddTicks(9771));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 13, 8, 55, 816, DateTimeKind.Utc).AddTicks(9773));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 13, 8, 55, 816, DateTimeKind.Utc).AddTicks(9774));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 13, 8, 55, 816, DateTimeKind.Utc).AddTicks(9775));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 13, 8, 55, 816, DateTimeKind.Utc).AddTicks(9776));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 13, 8, 55, 816, DateTimeKind.Utc).AddTicks(9777));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 13, 8, 55, 816, DateTimeKind.Utc).AddTicks(9778));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 13, 8, 55, 816, DateTimeKind.Utc).AddTicks(9780));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 16, 13, 8, 55, 816, DateTimeKind.Utc).AddTicks(9781));
        }
    }
}
