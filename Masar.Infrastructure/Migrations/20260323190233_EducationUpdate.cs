using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EducationUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── StartYear ─────────────────────────────────────────
            migrationBuilder.DropColumn(
                name: "StartYear",
                table: "Educations");

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartYear",
                table: "Educations",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            // ── ExpectedGraduation ────────────────────────────────
            migrationBuilder.DropColumn(
                name: "ExpectedGraduation",
                table: "Educations");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpectedGraduation",
                table: "Educations",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartYear",
                table: "Educations");

            migrationBuilder.AddColumn<int>(
                name: "StartYear",
                table: "Educations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.DropColumn(
                name: "ExpectedGraduation",
                table: "Educations");

            migrationBuilder.AddColumn<int>(
                name: "ExpectedGraduation",
                table: "Educations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
