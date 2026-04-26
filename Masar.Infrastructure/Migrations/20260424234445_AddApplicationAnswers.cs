using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationAnswers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobApplicationId = table.Column<int>(type: "int", nullable: false),
                    JobQuestionId = table.Column<int>(type: "int", nullable: false),
                    AnswerText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationAnswers_JobApplications_JobApplicationId",
                        column: x => x.JobApplicationId,
                        principalTable: "JobApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationAnswers_JobQuestions_JobQuestionId",
                        column: x => x.JobQuestionId,
                        principalTable: "JobQuestions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationAnswers_JobApplicationId",
                table: "ApplicationAnswers",
                column: "JobApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationAnswers_JobQuestionId",
                table: "ApplicationAnswers",
                column: "JobQuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationAnswers");
        }
    }
}
