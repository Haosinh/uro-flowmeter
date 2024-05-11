using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UroMeter.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class SettingColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Height",
                table: "MedicalRecordDatas");

            migrationBuilder.DropColumn(
                name: "Time",
                table: "MedicalRecordDatas");

            migrationBuilder.AddColumn<int>(
                name: "TimeInMilisecond",
                table: "MedicalRecordDatas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VolumnInMililiter",
                table: "MedicalRecordDatas",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeInMilisecond",
                table: "MedicalRecordDatas");

            migrationBuilder.DropColumn(
                name: "VolumnInMililiter",
                table: "MedicalRecordDatas");

            migrationBuilder.AddColumn<double>(
                name: "Height",
                table: "MedicalRecordDatas",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "Time",
                table: "MedicalRecordDatas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
