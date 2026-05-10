using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameCoverLetterUrlToText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CoverLetterUrl",
                table: "JobApplications",
                newName: "CoverLetter");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CoverLetter",
                table: "JobApplications",
                newName: "CoverLetterUrl");
        }
    }
}
