using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace studyAssistant.Web.Migrations
{
    public partial class ChangedStudyActivityToStudySession : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudyActivity");

            migrationBuilder.DropTable(
                name: "StudyActivityType");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Assignment",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<int>(
                name: "Grade",
                table: "Assignment",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Deadline",
                table: "Assignment",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "StudySessionType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudySessionType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudySession",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<int>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    Description = table.Column<string>(nullable: true),
                    Finish = table.Column<DateTime>(nullable: false),
                    IsCompleted = table.Column<bool>(nullable: true),
                    Start = table.Column<DateTime>(nullable: false),
                    StudySessionTypeId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudySession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudySession_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudySession_StudySessionType_StudySessionTypeId",
                        column: x => x.StudySessionTypeId,
                        principalTable: "StudySessionType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudySession_CourseId",
                table: "StudySession",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_StudySession_StudySessionTypeId",
                table: "StudySession",
                column: "StudySessionTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudySession");

            migrationBuilder.DropTable(
                name: "StudySessionType");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Assignment",
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "Grade",
                table: "Assignment",
                nullable: true,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Deadline",
                table: "Assignment",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.CreateTable(
                name: "StudyActivityType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyActivityType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StudyActivity",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourseId = table.Column<int>(nullable: false),
                    DateCreated = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    Description = table.Column<string>(nullable: true),
                    Finish = table.Column<DateTime>(nullable: false),
                    IsCompleted = table.Column<bool>(nullable: true),
                    Start = table.Column<DateTime>(nullable: false),
                    StudyActivityTypeId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyActivity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudyActivity_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudyActivity_StudyActivityType_StudyActivityTypeId",
                        column: x => x.StudyActivityTypeId,
                        principalTable: "StudyActivityType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudyActivity_CourseId",
                table: "StudyActivity",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyActivity_StudyActivityTypeId",
                table: "StudyActivity",
                column: "StudyActivityTypeId");
        }
    }
}
