using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class userupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Sectors",
                keyColumn: "Id",
                keyValue: 17,
                column: "Name",
                value: "Tribuna Lateral Izquierda");

            migrationBuilder.UpdateData(
                table: "Sectors",
                keyColumn: "Id",
                keyValue: 18,
                column: "Name",
                value: "Tribuna Lateral Derecha");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Name", "PasswordHash" },
                values: new object[,]
                {
                    { 2, "john.code@example.com", "John Code", "hashedPassword123" },
                    { 3, "maria.garcia@example.com", "Maria Garcia", "hashedPassword456" },
                    { 4, "carlos.rodriguez@example.com", "Carlos Rodriguez", "hashedPassword789" },
                    { 5, "sofia.martinez@example.com", "Sofia Martinez", "hashedPassword101112" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "Sectors",
                keyColumn: "Id",
                keyValue: 17,
                column: "Name",
                value: "Tribuna Lateral Norte");

            migrationBuilder.UpdateData(
                table: "Sectors",
                keyColumn: "Id",
                keyValue: 18,
                column: "Name",
                value: "Tribuna Lateral Sur");
        }
    }
}
