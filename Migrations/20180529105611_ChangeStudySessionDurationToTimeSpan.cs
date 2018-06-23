using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace studyAssistant.Web.Migrations
{
    public partial class ChangeStudySessionDurationToTimeSpan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "Duration",
                table: "StudySession",
                nullable: false,
                oldClrType: typeof(DateTime));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Duration",
                table: "StudySession",
                nullable: false,
                oldClrType: typeof(TimeSpan));
        }
    }
}
