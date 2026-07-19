using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncNullableFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Wallets",
                keyColumn: "Id",
                keyValue: new Guid("ce80ed0b-6db7-401c-b292-5dc8edc2ba2f"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastReadDate",
                table: "ReadingProgresses",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ReadingProgresses",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(sbyte),
                oldType: "tinyint(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<bool>(
                name: "IsCompleted",
                table: "ReadingProgresses",
                type: "tinyint(1)",
                nullable: false,
                oldClrType: typeof(sbyte),
                oldType: "tinyint(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "CurrentChapter",
                table: "ReadingProgresses",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Wallets",
                keyColumn: "Id",
                keyValue: new Guid("1a68d21f-f543-41b1-9c9f-92ecf61a8e17"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastReadDate",
                table: "ReadingProgresses",
                type: "datetime(6)",
                maxLength: 100,
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<sbyte>(
                name: "IsDeleted",
                table: "ReadingProgresses",
                type: "tinyint(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<sbyte>(
                name: "IsCompleted",
                table: "ReadingProgresses",
                type: "tinyint(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "tinyint(1)");

            migrationBuilder.AlterColumn<string>(
                name: "CurrentChapter",
                table: "ReadingProgresses",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("c117635d-96e0-409b-9fae-72976ec9c42a"),
                columns: new[] { "DateCreated", "HashPassword" },
                values: new object[] { new DateTime(2026, 7, 16, 11, 21, 21, 797, DateTimeKind.Utc).AddTicks(4161), "AQAAAAIAAYagAAAAEPJ7YSwsmyqzNvg54jRHpuHR6e9vsv2TnaBBDSHM2G2G9Lj7rOb6N2w3oea8UudUQw==" });

            migrationBuilder.InsertData(
                table: "Wallets",
                columns: new[] { "Id", "Balance", "CreatedBy", "DateCreated", "DateModified", "IsDeleted", "LastPayoutDate", "UserId" },
                values: new object[] { new Guid("ce80ed0b-6db7-401c-b292-5dc8edc2ba2f"), 0m, "admin@gmail.com", new DateTime(2026, 7, 16, 11, 21, 21, 901, DateTimeKind.Utc).AddTicks(9927), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("c117635d-96e0-409b-9fae-72976ec9c42a") });
        }
    }
}
