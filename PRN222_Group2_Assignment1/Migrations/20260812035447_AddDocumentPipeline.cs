using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN222_Group2_Assignment1.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentPipeline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 3, 54, 46, 824, DateTimeKind.Utc).AddTicks(2747));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 3, 54, 46, 824, DateTimeKind.Utc).AddTicks(2749));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 3, 54, 46, 824, DateTimeKind.Utc).AddTicks(2750));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 3, 54, 46, 824, DateTimeKind.Utc).AddTicks(2751));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 3, 54, 46, 824, DateTimeKind.Utc).AddTicks(2752));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 3, 54, 46, 824, DateTimeKind.Utc).AddTicks(2755));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 3, 54, 46, 824, DateTimeKind.Utc).AddTicks(2756));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 3, 54, 46, 824, DateTimeKind.Utc).AddTicks(2757));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 3, 54, 46, 824, DateTimeKind.Utc).AddTicks(2758));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 3, 54, 46, 824, DateTimeKind.Utc).AddTicks(2760));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 3, 36, 31, 758, DateTimeKind.Utc).AddTicks(1243));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 3, 36, 31, 758, DateTimeKind.Utc).AddTicks(1245));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 3, 36, 31, 758, DateTimeKind.Utc).AddTicks(1247));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 3, 36, 31, 758, DateTimeKind.Utc).AddTicks(1249));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 3, 36, 31, 758, DateTimeKind.Utc).AddTicks(1250));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 3, 36, 31, 758, DateTimeKind.Utc).AddTicks(1251));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 3, 36, 31, 758, DateTimeKind.Utc).AddTicks(1253));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 3, 36, 31, 758, DateTimeKind.Utc).AddTicks(1254));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 3, 36, 31, 758, DateTimeKind.Utc).AddTicks(1256));

            migrationBuilder.UpdateData(
                table: "DocumentChunks",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 12, 3, 36, 31, 758, DateTimeKind.Utc).AddTicks(1257));
        }
    }
}
