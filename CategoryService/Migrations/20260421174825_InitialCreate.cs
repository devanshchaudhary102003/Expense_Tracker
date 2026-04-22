using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CategoryService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "Color", "CreatedAt", "Icon", "IsActive", "IsDefault", "Name", "Type", "UserId" },
                values: new object[,]
                {
                    { 1, "#ef4444", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "🍔", true, true, "Food", "EXPENSE", null },
                    { 2, "#3b82f6", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "🚗", true, true, "Transport", "EXPENSE", null },
                    { 3, "#a855f7", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "🎬", true, true, "Entertainment", "EXPENSE", null },
                    { 4, "#10b981", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "💊", true, true, "Health", "EXPENSE", null },
                    { 5, "#f59e0b", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "🛍", true, true, "Shopping", "EXPENSE", null },
                    { 6, "#6366f1", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "💡", true, true, "Bills", "EXPENSE", null },
                    { 7, "#14b8a6", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "📚", true, true, "Education", "EXPENSE", null },
                    { 8, "#22c55e", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "💰", true, true, "Salary", "INCOME", null },
                    { 9, "#06b6d4", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "💻", true, true, "Freelance", "INCOME", null },
                    { 10, "#84cc16", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "📈", true, true, "Investment", "INCOME", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_UserId_Name",
                table: "Categories",
                columns: new[] { "UserId", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
