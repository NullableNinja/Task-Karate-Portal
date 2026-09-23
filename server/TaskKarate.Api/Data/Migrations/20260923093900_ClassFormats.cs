using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskKarate.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class ClassFormats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AppointmentOnly",
                table: "ClassTemplates",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ClassType",
                table: "ClassTemplates",
                type: "TEXT",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedStudentId",
                table: "ClassSessions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessions_AssignedStudentId",
                table: "ClassSessions",
                column: "AssignedStudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassSessions_Students_AssignedStudentId",
                table: "ClassSessions",
                column: "AssignedStudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassSessions_Students_AssignedStudentId",
                table: "ClassSessions");

            migrationBuilder.DropIndex(
                name: "IX_ClassSessions_AssignedStudentId",
                table: "ClassSessions");

            migrationBuilder.DropColumn(
                name: "AppointmentOnly",
                table: "ClassTemplates");

            migrationBuilder.DropColumn(
                name: "ClassType",
                table: "ClassTemplates");

            migrationBuilder.DropColumn(
                name: "AssignedStudentId",
                table: "ClassSessions");
        }
    }
}
