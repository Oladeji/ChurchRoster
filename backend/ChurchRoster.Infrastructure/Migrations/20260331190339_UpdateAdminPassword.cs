using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChurchRoster.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "user_id",
                keyValue: 1,
                column: "password_hash",
                value: "$2a$11$vZXm0xt/6JqZp4N0a2wVYO3h0i2EyKxGQ0rN0MqRXqB6Kd9Y3qH8G");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "user_id",
                keyValue: 1,
                column: "password_hash",
                value: "$2a$11$YourBCryptHashHere");
        }
    }
}
