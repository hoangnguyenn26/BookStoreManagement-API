using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookstore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAtUtc", "Email", "FirstName", "IsActive", "LastName", "PasswordHash", "PhoneNumber", "UpdatedAtUtc", "UserName" },
                values: new object[] { new Guid("f54527eb-f806-40db-bf76-c7b0e5fa6d39"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@bookstore.com", "Admin", true, "User", "$2a$12$PCb6JuQsMqxNkxzSLh1EaOaQBbtDy0wwOdu5xkSu7nbJ31KB8yRAe", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin" });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { new Guid("e1f3e5d4-1111-4f6f-9c5c-9b8d3a5b2a01"), new Guid("f54527eb-f806-40db-bf76-c7b0e5fa6d39") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("e1f3e5d4-1111-4f6f-9c5c-9b8d3a5b2a01"), new Guid("f54527eb-f806-40db-bf76-c7b0e5fa6d39") });

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("f54527eb-f806-40db-bf76-c7b0e5fa6d39"));
        }
    }
}
