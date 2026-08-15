using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN222_Group2_Assignment1.Migrations
{
    /// <inheritdoc />
    public partial class AddChatBotFlow2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SelectedDocumentIdsJson = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatSessions_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatSessions_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CitationsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_ChatSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "ChatSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SessionId",
                table: "ChatMessages",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_SubjectId",
                table: "ChatSessions",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_UserId",
                table: "ChatSessions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "ChatSessions");

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
    }
}
