using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UroMeter.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRecordAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CheckUpAt",
                table: "Records",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<DateTime>(
                name: "RecordAt",
                table: "Records",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecordAt",
                table: "Records");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CheckUpAt",
                table: "Records",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }
    }
}
