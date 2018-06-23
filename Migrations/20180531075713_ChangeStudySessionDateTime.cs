using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace studyAssistant.Web.Migrations
{
    public partial class ChangeStudySessionDateTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "StudySession",
                type: "time",
                nullable: false,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "StudySession",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "StudySession",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "time");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "StudySession",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");
        }
    }
}
