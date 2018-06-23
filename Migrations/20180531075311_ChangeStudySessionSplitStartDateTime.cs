using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace studyAssistant.Web.Migrations
{
    public partial class ChangeStudySessionSplitStartDateTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Start",
                table: "StudySession",
                newName: "StartTime");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "StudySession",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "StudySession");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "StudySession",
                newName: "Start");
        }
    }
}
