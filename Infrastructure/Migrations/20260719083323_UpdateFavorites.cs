using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFavorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_Books_ReaderId",
                table: "Favorites");

            migrationBuilder.DeleteData(
                table: "Wallets",
                keyColumn: "Id",
                keyValue: new Guid("1a68d21f-f543-41b1-9c9f-92ecf61a8e17"));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("c117635d-96e0-409b-9fae-72976ec9c42a"),
                columns: new[] { "DateCreated", "HashPassword" },
                values: new object[] { new DateTime(2026, 7, 19, 8, 33, 22, 554, DateTimeKind.Utc).AddTicks(2633), "AQAAAAIAAYagAAAAEIpWXQwzsWgMEaIWc2lH7gqR7/uYJJnShp9C3dHUSvM8iv2U7G6XCoarZqszkCn71w==" });

            migrationBuilder.InsertData(
                table: "Wallets",
                columns: new[] { "Id", "Balance", "CreatedBy", "DateCreated", "DateModified", "IsDeleted", "LastPayoutDate", "UserId" },
                values: new object[] { new Guid("3ec20f1b-13d8-4ff0-9784-de8af7b23cde"), 0m, "admin@gmail.com", new DateTime(2026, 7, 19, 8, 33, 22, 629, DateTimeKind.Utc).AddTicks(9874), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c117635d-96e0-409b-9fae-72976ec9c42a") });

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_BookId",
                table: "Favorites",
                column: "BookId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_Books_BookId",
                table: "Favorites",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_Books_BookId",
                table: "Favorites");

            migrationBuilder.DropIndex(
                name: "IX_Favorites_BookId",
                table: "Favorites");

            migrationBuilder.DeleteData(
                table: "Wallets",
                keyColumn: "Id",
                keyValue: new Guid("3ec20f1b-13d8-4ff0-9784-de8af7b23cde"));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("c117635d-96e0-409b-9fae-72976ec9c42a"),
                columns: new[] { "DateCreated", "HashPassword" },
                values: new object[] { new DateTime(2026, 7, 17, 15, 48, 40, 861, DateTimeKind.Utc).AddTicks(2879), "AQAAAAIAAYagAAAAEO9lNrO4Sy23h/sHKXHfyziBfBZEy3/5XHVxbt5Yd4876NxTT0i9xZeAh+MDepQTtA==" });

            migrationBuilder.InsertData(
                table: "Wallets",
                columns: new[] { "Id", "Balance", "CreatedBy", "DateCreated", "DateModified", "IsDeleted", "LastPayoutDate", "UserId" },
                values: new object[] { new Guid("1a68d21f-f543-41b1-9c9f-92ecf61a8e17"), 0m, "admin@gmail.com", new DateTime(2026, 7, 17, 15, 48, 40, 972, DateTimeKind.Utc).AddTicks(5179), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c117635d-96e0-409b-9fae-72976ec9c42a") });

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_Books_ReaderId",
                table: "Favorites",
                column: "ReaderId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
