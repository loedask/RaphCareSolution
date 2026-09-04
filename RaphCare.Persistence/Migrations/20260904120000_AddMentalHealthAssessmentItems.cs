using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RaphCare.Persistence;

#nullable disable

namespace RaphCare.Persistence.Migrations;

[DbContext(typeof(ClinicalDbContext))]
[Migration("20260904120000_AddMentalHealthAssessmentItems")]
public partial class AddMentalHealthAssessmentItems : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AssessmentQuestions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MentalHealthAssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                QuestionText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                Order = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AssessmentQuestions", x => x.Id);
                table.ForeignKey(
                    name: "FK_AssessmentQuestions_MentalHealthAssessments_MentalHealthAssessmentId",
                    column: x => x.MentalHealthAssessmentId,
                    principalTable: "MentalHealthAssessments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "AssessmentResponses",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MentalHealthAssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AssessmentQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ResponseValue = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                NumericScore = table.Column<int>(type: "int", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AssessmentResponses", x => x.Id);
                table.ForeignKey(
                    name: "FK_AssessmentResponses_AssessmentQuestions_AssessmentQuestionId",
                    column: x => x.AssessmentQuestionId,
                    principalTable: "AssessmentQuestions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_AssessmentResponses_MentalHealthAssessments_MentalHealthAssessmentId",
                    column: x => x.MentalHealthAssessmentId,
                    principalTable: "MentalHealthAssessments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AssessmentQuestions_MentalHealthAssessmentId",
            table: "AssessmentQuestions",
            column: "MentalHealthAssessmentId");

        migrationBuilder.CreateIndex(
            name: "IX_AssessmentQuestions_MentalHealthAssessmentId_Order",
            table: "AssessmentQuestions",
            columns: new[] { "MentalHealthAssessmentId", "Order" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AssessmentResponses_AssessmentQuestionId",
            table: "AssessmentResponses",
            column: "AssessmentQuestionId");

        migrationBuilder.CreateIndex(
            name: "IX_AssessmentResponses_MentalHealthAssessmentId",
            table: "AssessmentResponses",
            column: "MentalHealthAssessmentId");

        migrationBuilder.CreateIndex(
            name: "IX_AssessmentResponses_MentalHealthAssessmentId_AssessmentQuestionId",
            table: "AssessmentResponses",
            columns: new[] { "MentalHealthAssessmentId", "AssessmentQuestionId" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AssessmentResponses");
        migrationBuilder.DropTable(name: "AssessmentQuestions");
    }
}
