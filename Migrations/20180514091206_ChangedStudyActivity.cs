using Microsoft.EntityFrameworkCore.Migrations;

namespace studyAssistant.Web.Migrations
{
    public partial class ChangedStudyActivity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudyActivity_Course_CourseId1",
                table: "StudyActivity");

            migrationBuilder.DropIndex(
                name: "IX_StudyActivity_CourseId1",
                table: "StudyActivity");

            migrationBuilder.DropColumn(
                name: "CourseId1",
                table: "StudyActivity");

            migrationBuilder.AlterColumn<int>(
                name: "CourseId",
                table: "StudyActivity",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.CreateIndex(
                name: "IX_StudyActivity_CourseId",
                table: "StudyActivity",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudyActivity_Course_CourseId",
                table: "StudyActivity",
                column: "CourseId",
                principalTable: "Course",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudyActivity_Course_CourseId",
                table: "StudyActivity");

            migrationBuilder.DropIndex(
                name: "IX_StudyActivity_CourseId",
                table: "StudyActivity");

            migrationBuilder.AlterColumn<long>(
                name: "CourseId",
                table: "StudyActivity",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<int>(
                name: "CourseId1",
                table: "StudyActivity",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudyActivity_CourseId1",
                table: "StudyActivity",
                column: "CourseId1");

            migrationBuilder.AddForeignKey(
                name: "FK_StudyActivity_Course_CourseId1",
                table: "StudyActivity",
                column: "CourseId1",
                principalTable: "Course",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
