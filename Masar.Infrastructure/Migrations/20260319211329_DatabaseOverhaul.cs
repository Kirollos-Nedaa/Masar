using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseOverhaul : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfessionalLinks_CandidateProfiles_CandidateProfileId",
                table: "ProfessionalLinks");

            migrationBuilder.DropIndex(
                name: "IX_Skills_Name",
                table: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_ProfessionalLinks_CandidateProfileId",
                table: "ProfessionalLinks");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GitHubUrl",
                table: "ProfessionalLinks");

            migrationBuilder.DropColumn(
                name: "LinkedInUrl",
                table: "ProfessionalLinks");

            migrationBuilder.DropColumn(
                name: "PortfolioUrl",
                table: "ProfessionalLinks");

            migrationBuilder.DropColumn(
                name: "Views",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "ProfileViews",
                table: "CandidateProfiles");

            migrationBuilder.RenameColumn(
                name: "CompanyName",
                table: "CompanyProfiles",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "CompanyLogo",
                table: "CompanyProfiles",
                newName: "LogoUrl");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "CompanyProfileId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Skills",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SavedAt",
                table: "SavedJobs",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "CandidateProfileId",
                table: "ProfessionalLinks",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CompanyProfileId",
                table: "ProfessionalLinks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LinksNames",
                table: "ProfessionalLinks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "ProfessionalLinks",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "WorkMode",
                table: "Jobs",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Requirements",
                table: "Jobs",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3000)",
                oldMaxLength: 3000);

            migrationBuilder.AlterColumn<int>(
                name: "JobType",
                table: "Jobs",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApplicationDeadline",
                table: "Jobs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Benefits",
                table: "Jobs",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Department",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfOpenings",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "RequireCoverLetter",
                table: "Jobs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireCv",
                table: "Jobs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CoverLetterUrl",
                table: "JobApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResumeUrl",
                table: "JobApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Industry",
                table: "CompanyProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Size",
                table: "CompanyProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ResumeUrl",
                table: "CandidateProfiles",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "CandidateProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "CandidateProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "CandidateProfiles",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "CandidateProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "CandidateProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CompanyContactInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyProfileId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyContactInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyContactInfos_CompanyProfiles_CompanyProfileId",
                        column: x => x.CompanyProfileId,
                        principalTable: "CompanyProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobQuestions_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanyProfileId",
                table: "Users",
                column: "CompanyProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Skills_NormalizedName",
                table: "Skills",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavedJobs_CandidateProfileId",
                table: "SavedJobs",
                column: "CandidateProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfessionalLinks_CandidateProfileId",
                table: "ProfessionalLinks",
                column: "CandidateProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfessionalLinks_CompanyProfileId",
                table: "ProfessionalLinks",
                column: "CompanyProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Department",
                table: "Jobs",
                column: "Department");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_JobType",
                table: "Jobs",
                column: "JobType");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Location",
                table: "Jobs",
                column: "Location");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_PostedDate",
                table: "Jobs",
                column: "PostedDate");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateProfiles_Location",
                table: "CandidateProfiles",
                column: "Location");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyContactInfos_CompanyProfileId",
                table: "CompanyContactInfos",
                column: "CompanyProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobQuestions_JobId",
                table: "JobQuestions",
                column: "JobId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfessionalLinks_CandidateProfiles_CandidateProfileId",
                table: "ProfessionalLinks",
                column: "CandidateProfileId",
                principalTable: "CandidateProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfessionalLinks_CompanyProfiles_CompanyProfileId",
                table: "ProfessionalLinks",
                column: "CompanyProfileId",
                principalTable: "CompanyProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_CompanyProfiles_CompanyProfileId",
                table: "Users",
                column: "CompanyProfileId",
                principalTable: "CompanyProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfessionalLinks_CandidateProfiles_CandidateProfileId",
                table: "ProfessionalLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_ProfessionalLinks_CompanyProfiles_CompanyProfileId",
                table: "ProfessionalLinks");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_CompanyProfiles_CompanyProfileId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "CompanyContactInfos");

            migrationBuilder.DropTable(
                name: "JobQuestions");

            migrationBuilder.DropIndex(
                name: "IX_Users_CompanyProfileId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Skills_NormalizedName",
                table: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_SavedJobs_CandidateProfileId",
                table: "SavedJobs");

            migrationBuilder.DropIndex(
                name: "IX_ProfessionalLinks_CandidateProfileId",
                table: "ProfessionalLinks");

            migrationBuilder.DropIndex(
                name: "IX_ProfessionalLinks_CompanyProfileId",
                table: "ProfessionalLinks");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_Department",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_JobType",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_Location",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_PostedDate",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_CandidateProfiles_Location",
                table: "CandidateProfiles");

            migrationBuilder.DropColumn(
                name: "CompanyProfileId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "CompanyProfileId",
                table: "ProfessionalLinks");

            migrationBuilder.DropColumn(
                name: "LinksNames",
                table: "ProfessionalLinks");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "ProfessionalLinks");

            migrationBuilder.DropColumn(
                name: "ApplicationDeadline",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Benefits",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "NumberOfOpenings",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "RequireCoverLetter",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "RequireCv",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "CoverLetterUrl",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "ResumeUrl",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "Industry",
                table: "CompanyProfiles");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "CompanyProfiles");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "CandidateProfiles");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "CandidateProfiles");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "CandidateProfiles");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "CompanyProfiles",
                newName: "CompanyName");

            migrationBuilder.RenameColumn(
                name: "LogoUrl",
                table: "CompanyProfiles",
                newName: "CompanyLogo");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "Users",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "SavedAt",
                table: "SavedJobs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "CandidateProfileId",
                table: "ProfessionalLinks",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GitHubUrl",
                table: "ProfessionalLinks",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LinkedInUrl",
                table: "ProfessionalLinks",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PortfolioUrl",
                table: "ProfessionalLinks",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "WorkMode",
                table: "Jobs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Requirements",
                table: "Jobs",
                type: "nvarchar(3000)",
                maxLength: 3000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AlterColumn<string>(
                name: "JobType",
                table: "Jobs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "Views",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "ResumeUrl",
                table: "CandidateProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "CandidateProfiles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "CandidateProfiles",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProfileViews",
                table: "CandidateProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Skills_Name",
                table: "Skills",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfessionalLinks_CandidateProfileId",
                table: "ProfessionalLinks",
                column: "CandidateProfileId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProfessionalLinks_CandidateProfiles_CandidateProfileId",
                table: "ProfessionalLinks",
                column: "CandidateProfileId",
                principalTable: "CandidateProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
