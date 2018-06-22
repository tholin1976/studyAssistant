using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace StudyAssistant.Web.Migrations
{
    public partial class ChangedFinishDateToDurationStudySession : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Finish",
                table: "StudySession",
                newName: "Duration");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "StudySession",
                newName: "Finish");
        }
    }
}
