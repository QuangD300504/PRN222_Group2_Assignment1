using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRN222_Group2_Assignment1.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Email", "FullName", "Password", "Role" },
                values: new object[] { "leader@chatbot.edu.vn", "Subject Leader", "leader@123", "SubjectLeader" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Email", "FullName", "Password", "Role" },
                values: new object[] { "admin@chatbot.edu.vn", "Administrator", "admin@123", "Admin" });
        }
    }
}
