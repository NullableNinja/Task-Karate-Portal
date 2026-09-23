using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskKarate.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class NamespacePlatformTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_ClassSessions_ClassSessionId",
                table: "AttendanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Students_StudentId",
                table: "AttendanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassSessions_ClassTemplates_ClassTemplateId",
                table: "ClassSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassSessions_Students_AssignedStudentId",
                table: "ClassSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassTemplates_Programs_ProgramAreaId",
                table: "ClassTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsentAcceptances_ConsentDocuments_ConsentDocumentId",
                table: "ConsentAcceptances");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsentAcceptances_Guardians_GuardianId",
                table: "ConsentAcceptances");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsentAcceptances_Students_StudentId",
                table: "ConsentAcceptances");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_ClassTemplates_ClassTemplateId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Students_StudentId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_GuardianStudents_Guardians_GuardianId",
                table: "GuardianStudents");

            migrationBuilder.DropForeignKey(
                name: "FK_GuardianStudents_Students_StudentId",
                table: "GuardianStudents");

            migrationBuilder.DropForeignKey(
                name: "FK_RankRequirements_BeltRanks_BeltRankId",
                table: "RankRequirements");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentRankHistory_BeltRanks_BeltRankId",
                table: "StudentRankHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentRankHistory_Students_StudentId",
                table: "StudentRankHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentRequirementProgress_RankRequirements_RankRequirementId",
                table: "StudentRequirementProgress");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentRequirementProgress_Students_StudentId",
                table: "StudentRequirementProgress");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_BeltRanks_BeltRankId",
                table: "Students");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Students",
                table: "Students");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentRequirementProgress",
                table: "StudentRequirementProgress");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentRankHistory",
                table: "StudentRankHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RankRequirements",
                table: "RankRequirements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Programs",
                table: "Programs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NewsPosts",
                table: "NewsPosts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MediaAssets",
                table: "MediaAssets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuardianStudents",
                table: "GuardianStudents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Guardians",
                table: "Guardians");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Enrollments",
                table: "Enrollments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConsentDocuments",
                table: "ConsentDocuments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClassTemplates",
                table: "ClassTemplates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClassSessions",
                table: "ClassSessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BeltRanks",
                table: "BeltRanks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AuditEvents",
                table: "AuditEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AttendanceRecords",
                table: "AttendanceRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserTokens",
                table: "AspNetUserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetUserClaims",
                table: "AspNetUserClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoles",
                table: "AspNetRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AspNetRoleClaims",
                table: "AspNetRoleClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Announcements",
                table: "Announcements");

            migrationBuilder.RenameTable(
                name: "Students",
                newName: "platform_Students");

            migrationBuilder.RenameTable(
                name: "StudentRequirementProgress",
                newName: "platform_StudentRequirementProgress");

            migrationBuilder.RenameTable(
                name: "StudentRankHistory",
                newName: "platform_StudentRankHistory");

            migrationBuilder.RenameTable(
                name: "RankRequirements",
                newName: "platform_RankRequirements");

            migrationBuilder.RenameTable(
                name: "Programs",
                newName: "platform_Programs");

            migrationBuilder.RenameTable(
                name: "NewsPosts",
                newName: "platform_NewsPosts");

            migrationBuilder.RenameTable(
                name: "MediaAssets",
                newName: "platform_MediaAssets");

            migrationBuilder.RenameTable(
                name: "GuardianStudents",
                newName: "platform_GuardianStudents");

            migrationBuilder.RenameTable(
                name: "Guardians",
                newName: "platform_Guardians");

            migrationBuilder.RenameTable(
                name: "Enrollments",
                newName: "platform_Enrollments");

            migrationBuilder.RenameTable(
                name: "ConsentDocuments",
                newName: "platform_ConsentDocuments");

            migrationBuilder.RenameTable(
                name: "ClassTemplates",
                newName: "platform_ClassTemplates");

            migrationBuilder.RenameTable(
                name: "ClassSessions",
                newName: "platform_ClassSessions");

            migrationBuilder.RenameTable(
                name: "BeltRanks",
                newName: "platform_BeltRanks");

            migrationBuilder.RenameTable(
                name: "AuditEvents",
                newName: "platform_AuditEvents");

            migrationBuilder.RenameTable(
                name: "AttendanceRecords",
                newName: "platform_AttendanceRecords");

            migrationBuilder.RenameTable(
                name: "AspNetUserTokens",
                newName: "platform_AspNetUserTokens");

            migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "platform_AspNetUsers");

            migrationBuilder.RenameTable(
                name: "AspNetUserRoles",
                newName: "platform_AspNetUserRoles");

            migrationBuilder.RenameTable(
                name: "AspNetUserLogins",
                newName: "platform_AspNetUserLogins");

            migrationBuilder.RenameTable(
                name: "AspNetUserClaims",
                newName: "platform_AspNetUserClaims");

            migrationBuilder.RenameTable(
                name: "AspNetRoles",
                newName: "platform_AspNetRoles");

            migrationBuilder.RenameTable(
                name: "AspNetRoleClaims",
                newName: "platform_AspNetRoleClaims");

            migrationBuilder.RenameTable(
                name: "Announcements",
                newName: "platform_Announcements");

            migrationBuilder.RenameIndex(
                name: "IX_Students_IsActive_LastName_FirstName",
                table: "platform_Students",
                newName: "IX_platform_Students_IsActive_LastName_FirstName");

            migrationBuilder.RenameIndex(
                name: "IX_Students_BeltRankId",
                table: "platform_Students",
                newName: "IX_platform_Students_BeltRankId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentRequirementProgress_StudentId_RankRequirementId",
                table: "platform_StudentRequirementProgress",
                newName: "IX_platform_StudentRequirementProgress_StudentId_RankRequirementId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentRequirementProgress_RankRequirementId",
                table: "platform_StudentRequirementProgress",
                newName: "IX_platform_StudentRequirementProgress_RankRequirementId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentRankHistory_StudentId_AwardedAtUtc",
                table: "platform_StudentRankHistory",
                newName: "IX_platform_StudentRankHistory_StudentId_AwardedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_StudentRankHistory_BeltRankId",
                table: "platform_StudentRankHistory",
                newName: "IX_platform_StudentRankHistory_BeltRankId");

            migrationBuilder.RenameIndex(
                name: "IX_RankRequirements_BeltRankId_SortOrder",
                table: "platform_RankRequirements",
                newName: "IX_platform_RankRequirements_BeltRankId_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_Programs_IsActive_Name",
                table: "platform_Programs",
                newName: "IX_platform_Programs_IsActive_Name");

            migrationBuilder.RenameIndex(
                name: "IX_NewsPosts_Status_PublishedAtUtc",
                table: "platform_NewsPosts",
                newName: "IX_platform_NewsPosts_Status_PublishedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_NewsPosts_Slug",
                table: "platform_NewsPosts",
                newName: "IX_platform_NewsPosts_Slug");

            migrationBuilder.RenameIndex(
                name: "IX_GuardianStudents_StudentId",
                table: "platform_GuardianStudents",
                newName: "IX_platform_GuardianStudents_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Guardians_IsActive_LastName",
                table: "platform_Guardians",
                newName: "IX_platform_Guardians_IsActive_LastName");

            migrationBuilder.RenameIndex(
                name: "IX_Guardians_Email",
                table: "platform_Guardians",
                newName: "IX_platform_Guardians_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollments_StudentId_ClassTemplateId",
                table: "platform_Enrollments",
                newName: "IX_platform_Enrollments_StudentId_ClassTemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollments_ClassTemplateId",
                table: "platform_Enrollments",
                newName: "IX_platform_Enrollments_ClassTemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_ConsentDocuments_Name_Version",
                table: "platform_ConsentDocuments",
                newName: "IX_platform_ConsentDocuments_Name_Version");

            migrationBuilder.RenameIndex(
                name: "IX_ClassTemplates_ProgramAreaId",
                table: "platform_ClassTemplates",
                newName: "IX_platform_ClassTemplates_ProgramAreaId");

            migrationBuilder.RenameIndex(
                name: "IX_ClassTemplates_IsActive_DayOfWeek_StartTime",
                table: "platform_ClassTemplates",
                newName: "IX_platform_ClassTemplates_IsActive_DayOfWeek_StartTime");

            migrationBuilder.RenameIndex(
                name: "IX_ClassSessions_SessionDateUtc",
                table: "platform_ClassSessions",
                newName: "IX_platform_ClassSessions_SessionDateUtc");

            migrationBuilder.RenameIndex(
                name: "IX_ClassSessions_ClassTemplateId_SessionDateUtc",
                table: "platform_ClassSessions",
                newName: "IX_platform_ClassSessions_ClassTemplateId_SessionDateUtc");

            migrationBuilder.RenameIndex(
                name: "IX_ClassSessions_AssignedStudentId",
                table: "platform_ClassSessions",
                newName: "IX_platform_ClassSessions_AssignedStudentId");

            migrationBuilder.RenameIndex(
                name: "IX_BeltRanks_SortOrder_Name",
                table: "platform_BeltRanks",
                newName: "IX_platform_BeltRanks_SortOrder_Name");

            migrationBuilder.RenameIndex(
                name: "IX_AuditEvents_Entity_EntityId_OccurredAtUtc",
                table: "platform_AuditEvents",
                newName: "IX_platform_AuditEvents_Entity_EntityId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_AttendanceRecords_StudentId",
                table: "platform_AttendanceRecords",
                newName: "IX_platform_AttendanceRecords_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_AttendanceRecords_ClassSessionId_StudentId",
                table: "platform_AttendanceRecords",
                newName: "IX_platform_AttendanceRecords_ClassSessionId_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "platform_AspNetUserRoles",
                newName: "IX_platform_AspNetUserRoles_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "platform_AspNetUserLogins",
                newName: "IX_platform_AspNetUserLogins_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "platform_AspNetUserClaims",
                newName: "IX_platform_AspNetUserClaims_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "platform_AspNetRoleClaims",
                newName: "IX_platform_AspNetRoleClaims_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_Announcements_Status_PublishedAtUtc",
                table: "platform_Announcements",
                newName: "IX_platform_Announcements_Status_PublishedAtUtc");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_Students",
                table: "platform_Students",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_StudentRequirementProgress",
                table: "platform_StudentRequirementProgress",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_StudentRankHistory",
                table: "platform_StudentRankHistory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_RankRequirements",
                table: "platform_RankRequirements",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_Programs",
                table: "platform_Programs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_NewsPosts",
                table: "platform_NewsPosts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_MediaAssets",
                table: "platform_MediaAssets",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_GuardianStudents",
                table: "platform_GuardianStudents",
                columns: new[] { "GuardianId", "StudentId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_Guardians",
                table: "platform_Guardians",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_Enrollments",
                table: "platform_Enrollments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_ConsentDocuments",
                table: "platform_ConsentDocuments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_ClassTemplates",
                table: "platform_ClassTemplates",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_ClassSessions",
                table: "platform_ClassSessions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_BeltRanks",
                table: "platform_BeltRanks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_AuditEvents",
                table: "platform_AuditEvents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_AttendanceRecords",
                table: "platform_AttendanceRecords",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_AspNetUserTokens",
                table: "platform_AspNetUserTokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_AspNetUsers",
                table: "platform_AspNetUsers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_AspNetUserRoles",
                table: "platform_AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_AspNetUserLogins",
                table: "platform_AspNetUserLogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_AspNetUserClaims",
                table: "platform_AspNetUserClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_AspNetRoles",
                table: "platform_AspNetRoles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_AspNetRoleClaims",
                table: "platform_AspNetRoleClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_platform_Announcements",
                table: "platform_Announcements",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentAcceptances_platform_ConsentDocuments_ConsentDocumentId",
                table: "ConsentAcceptances",
                column: "ConsentDocumentId",
                principalTable: "platform_ConsentDocuments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentAcceptances_platform_Guardians_GuardianId",
                table: "ConsentAcceptances",
                column: "GuardianId",
                principalTable: "platform_Guardians",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentAcceptances_platform_Students_StudentId",
                table: "ConsentAcceptances",
                column: "StudentId",
                principalTable: "platform_Students",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_platform_AspNetRoleClaims_platform_AspNetRoles_RoleId",
                table: "platform_AspNetRoleClaims",
                column: "RoleId",
                principalTable: "platform_AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_AspNetUserClaims_platform_AspNetUsers_UserId",
                table: "platform_AspNetUserClaims",
                column: "UserId",
                principalTable: "platform_AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_AspNetUserLogins_platform_AspNetUsers_UserId",
                table: "platform_AspNetUserLogins",
                column: "UserId",
                principalTable: "platform_AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_AspNetUserRoles_platform_AspNetRoles_RoleId",
                table: "platform_AspNetUserRoles",
                column: "RoleId",
                principalTable: "platform_AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_AspNetUserRoles_platform_AspNetUsers_UserId",
                table: "platform_AspNetUserRoles",
                column: "UserId",
                principalTable: "platform_AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_AspNetUserTokens_platform_AspNetUsers_UserId",
                table: "platform_AspNetUserTokens",
                column: "UserId",
                principalTable: "platform_AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_AttendanceRecords_platform_ClassSessions_ClassSessionId",
                table: "platform_AttendanceRecords",
                column: "ClassSessionId",
                principalTable: "platform_ClassSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_AttendanceRecords_platform_Students_StudentId",
                table: "platform_AttendanceRecords",
                column: "StudentId",
                principalTable: "platform_Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_ClassSessions_platform_ClassTemplates_ClassTemplateId",
                table: "platform_ClassSessions",
                column: "ClassTemplateId",
                principalTable: "platform_ClassTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_ClassSessions_platform_Students_AssignedStudentId",
                table: "platform_ClassSessions",
                column: "AssignedStudentId",
                principalTable: "platform_Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_ClassTemplates_platform_Programs_ProgramAreaId",
                table: "platform_ClassTemplates",
                column: "ProgramAreaId",
                principalTable: "platform_Programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_Enrollments_platform_ClassTemplates_ClassTemplateId",
                table: "platform_Enrollments",
                column: "ClassTemplateId",
                principalTable: "platform_ClassTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_Enrollments_platform_Students_StudentId",
                table: "platform_Enrollments",
                column: "StudentId",
                principalTable: "platform_Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_GuardianStudents_platform_Guardians_GuardianId",
                table: "platform_GuardianStudents",
                column: "GuardianId",
                principalTable: "platform_Guardians",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_GuardianStudents_platform_Students_StudentId",
                table: "platform_GuardianStudents",
                column: "StudentId",
                principalTable: "platform_Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_RankRequirements_platform_BeltRanks_BeltRankId",
                table: "platform_RankRequirements",
                column: "BeltRankId",
                principalTable: "platform_BeltRanks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_StudentRankHistory_platform_BeltRanks_BeltRankId",
                table: "platform_StudentRankHistory",
                column: "BeltRankId",
                principalTable: "platform_BeltRanks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_StudentRankHistory_platform_Students_StudentId",
                table: "platform_StudentRankHistory",
                column: "StudentId",
                principalTable: "platform_Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_StudentRequirementProgress_platform_RankRequirements_RankRequirementId",
                table: "platform_StudentRequirementProgress",
                column: "RankRequirementId",
                principalTable: "platform_RankRequirements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_StudentRequirementProgress_platform_Students_StudentId",
                table: "platform_StudentRequirementProgress",
                column: "StudentId",
                principalTable: "platform_Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_platform_Students_platform_BeltRanks_BeltRankId",
                table: "platform_Students",
                column: "BeltRankId",
                principalTable: "platform_BeltRanks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsentAcceptances_platform_ConsentDocuments_ConsentDocumentId",
                table: "ConsentAcceptances");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsentAcceptances_platform_Guardians_GuardianId",
                table: "ConsentAcceptances");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsentAcceptances_platform_Students_StudentId",
                table: "ConsentAcceptances");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_AspNetRoleClaims_platform_AspNetRoles_RoleId",
                table: "platform_AspNetRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_AspNetUserClaims_platform_AspNetUsers_UserId",
                table: "platform_AspNetUserClaims");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_AspNetUserLogins_platform_AspNetUsers_UserId",
                table: "platform_AspNetUserLogins");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_AspNetUserRoles_platform_AspNetRoles_RoleId",
                table: "platform_AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_AspNetUserRoles_platform_AspNetUsers_UserId",
                table: "platform_AspNetUserRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_AspNetUserTokens_platform_AspNetUsers_UserId",
                table: "platform_AspNetUserTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_AttendanceRecords_platform_ClassSessions_ClassSessionId",
                table: "platform_AttendanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_AttendanceRecords_platform_Students_StudentId",
                table: "platform_AttendanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_ClassSessions_platform_ClassTemplates_ClassTemplateId",
                table: "platform_ClassSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_ClassSessions_platform_Students_AssignedStudentId",
                table: "platform_ClassSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_ClassTemplates_platform_Programs_ProgramAreaId",
                table: "platform_ClassTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_Enrollments_platform_ClassTemplates_ClassTemplateId",
                table: "platform_Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_Enrollments_platform_Students_StudentId",
                table: "platform_Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_GuardianStudents_platform_Guardians_GuardianId",
                table: "platform_GuardianStudents");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_GuardianStudents_platform_Students_StudentId",
                table: "platform_GuardianStudents");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_RankRequirements_platform_BeltRanks_BeltRankId",
                table: "platform_RankRequirements");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_StudentRankHistory_platform_BeltRanks_BeltRankId",
                table: "platform_StudentRankHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_StudentRankHistory_platform_Students_StudentId",
                table: "platform_StudentRankHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_StudentRequirementProgress_platform_RankRequirements_RankRequirementId",
                table: "platform_StudentRequirementProgress");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_StudentRequirementProgress_platform_Students_StudentId",
                table: "platform_StudentRequirementProgress");

            migrationBuilder.DropForeignKey(
                name: "FK_platform_Students_platform_BeltRanks_BeltRankId",
                table: "platform_Students");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_Students",
                table: "platform_Students");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_StudentRequirementProgress",
                table: "platform_StudentRequirementProgress");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_StudentRankHistory",
                table: "platform_StudentRankHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_RankRequirements",
                table: "platform_RankRequirements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_Programs",
                table: "platform_Programs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_NewsPosts",
                table: "platform_NewsPosts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_MediaAssets",
                table: "platform_MediaAssets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_GuardianStudents",
                table: "platform_GuardianStudents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_Guardians",
                table: "platform_Guardians");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_Enrollments",
                table: "platform_Enrollments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_ConsentDocuments",
                table: "platform_ConsentDocuments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_ClassTemplates",
                table: "platform_ClassTemplates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_ClassSessions",
                table: "platform_ClassSessions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_BeltRanks",
                table: "platform_BeltRanks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_AuditEvents",
                table: "platform_AuditEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_AttendanceRecords",
                table: "platform_AttendanceRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_AspNetUserTokens",
                table: "platform_AspNetUserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_AspNetUsers",
                table: "platform_AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_AspNetUserRoles",
                table: "platform_AspNetUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_AspNetUserLogins",
                table: "platform_AspNetUserLogins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_AspNetUserClaims",
                table: "platform_AspNetUserClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_AspNetRoles",
                table: "platform_AspNetRoles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_AspNetRoleClaims",
                table: "platform_AspNetRoleClaims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_platform_Announcements",
                table: "platform_Announcements");

            migrationBuilder.RenameTable(
                name: "platform_Students",
                newName: "Students");

            migrationBuilder.RenameTable(
                name: "platform_StudentRequirementProgress",
                newName: "StudentRequirementProgress");

            migrationBuilder.RenameTable(
                name: "platform_StudentRankHistory",
                newName: "StudentRankHistory");

            migrationBuilder.RenameTable(
                name: "platform_RankRequirements",
                newName: "RankRequirements");

            migrationBuilder.RenameTable(
                name: "platform_Programs",
                newName: "Programs");

            migrationBuilder.RenameTable(
                name: "platform_NewsPosts",
                newName: "NewsPosts");

            migrationBuilder.RenameTable(
                name: "platform_MediaAssets",
                newName: "MediaAssets");

            migrationBuilder.RenameTable(
                name: "platform_GuardianStudents",
                newName: "GuardianStudents");

            migrationBuilder.RenameTable(
                name: "platform_Guardians",
                newName: "Guardians");

            migrationBuilder.RenameTable(
                name: "platform_Enrollments",
                newName: "Enrollments");

            migrationBuilder.RenameTable(
                name: "platform_ConsentDocuments",
                newName: "ConsentDocuments");

            migrationBuilder.RenameTable(
                name: "platform_ClassTemplates",
                newName: "ClassTemplates");

            migrationBuilder.RenameTable(
                name: "platform_ClassSessions",
                newName: "ClassSessions");

            migrationBuilder.RenameTable(
                name: "platform_BeltRanks",
                newName: "BeltRanks");

            migrationBuilder.RenameTable(
                name: "platform_AuditEvents",
                newName: "AuditEvents");

            migrationBuilder.RenameTable(
                name: "platform_AttendanceRecords",
                newName: "AttendanceRecords");

            migrationBuilder.RenameTable(
                name: "platform_AspNetUserTokens",
                newName: "AspNetUserTokens");

            migrationBuilder.RenameTable(
                name: "platform_AspNetUsers",
                newName: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "platform_AspNetUserRoles",
                newName: "AspNetUserRoles");

            migrationBuilder.RenameTable(
                name: "platform_AspNetUserLogins",
                newName: "AspNetUserLogins");

            migrationBuilder.RenameTable(
                name: "platform_AspNetUserClaims",
                newName: "AspNetUserClaims");

            migrationBuilder.RenameTable(
                name: "platform_AspNetRoles",
                newName: "AspNetRoles");

            migrationBuilder.RenameTable(
                name: "platform_AspNetRoleClaims",
                newName: "AspNetRoleClaims");

            migrationBuilder.RenameTable(
                name: "platform_Announcements",
                newName: "Announcements");

            migrationBuilder.RenameIndex(
                name: "IX_platform_Students_IsActive_LastName_FirstName",
                table: "Students",
                newName: "IX_Students_IsActive_LastName_FirstName");

            migrationBuilder.RenameIndex(
                name: "IX_platform_Students_BeltRankId",
                table: "Students",
                newName: "IX_Students_BeltRankId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_StudentRequirementProgress_StudentId_RankRequirementId",
                table: "StudentRequirementProgress",
                newName: "IX_StudentRequirementProgress_StudentId_RankRequirementId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_StudentRequirementProgress_RankRequirementId",
                table: "StudentRequirementProgress",
                newName: "IX_StudentRequirementProgress_RankRequirementId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_StudentRankHistory_StudentId_AwardedAtUtc",
                table: "StudentRankHistory",
                newName: "IX_StudentRankHistory_StudentId_AwardedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_platform_StudentRankHistory_BeltRankId",
                table: "StudentRankHistory",
                newName: "IX_StudentRankHistory_BeltRankId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_RankRequirements_BeltRankId_SortOrder",
                table: "RankRequirements",
                newName: "IX_RankRequirements_BeltRankId_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_platform_Programs_IsActive_Name",
                table: "Programs",
                newName: "IX_Programs_IsActive_Name");

            migrationBuilder.RenameIndex(
                name: "IX_platform_NewsPosts_Status_PublishedAtUtc",
                table: "NewsPosts",
                newName: "IX_NewsPosts_Status_PublishedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_platform_NewsPosts_Slug",
                table: "NewsPosts",
                newName: "IX_NewsPosts_Slug");

            migrationBuilder.RenameIndex(
                name: "IX_platform_GuardianStudents_StudentId",
                table: "GuardianStudents",
                newName: "IX_GuardianStudents_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_Guardians_IsActive_LastName",
                table: "Guardians",
                newName: "IX_Guardians_IsActive_LastName");

            migrationBuilder.RenameIndex(
                name: "IX_platform_Guardians_Email",
                table: "Guardians",
                newName: "IX_Guardians_Email");

            migrationBuilder.RenameIndex(
                name: "IX_platform_Enrollments_StudentId_ClassTemplateId",
                table: "Enrollments",
                newName: "IX_Enrollments_StudentId_ClassTemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_Enrollments_ClassTemplateId",
                table: "Enrollments",
                newName: "IX_Enrollments_ClassTemplateId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_ConsentDocuments_Name_Version",
                table: "ConsentDocuments",
                newName: "IX_ConsentDocuments_Name_Version");

            migrationBuilder.RenameIndex(
                name: "IX_platform_ClassTemplates_ProgramAreaId",
                table: "ClassTemplates",
                newName: "IX_ClassTemplates_ProgramAreaId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_ClassTemplates_IsActive_DayOfWeek_StartTime",
                table: "ClassTemplates",
                newName: "IX_ClassTemplates_IsActive_DayOfWeek_StartTime");

            migrationBuilder.RenameIndex(
                name: "IX_platform_ClassSessions_SessionDateUtc",
                table: "ClassSessions",
                newName: "IX_ClassSessions_SessionDateUtc");

            migrationBuilder.RenameIndex(
                name: "IX_platform_ClassSessions_ClassTemplateId_SessionDateUtc",
                table: "ClassSessions",
                newName: "IX_ClassSessions_ClassTemplateId_SessionDateUtc");

            migrationBuilder.RenameIndex(
                name: "IX_platform_ClassSessions_AssignedStudentId",
                table: "ClassSessions",
                newName: "IX_ClassSessions_AssignedStudentId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_BeltRanks_SortOrder_Name",
                table: "BeltRanks",
                newName: "IX_BeltRanks_SortOrder_Name");

            migrationBuilder.RenameIndex(
                name: "IX_platform_AuditEvents_Entity_EntityId_OccurredAtUtc",
                table: "AuditEvents",
                newName: "IX_AuditEvents_Entity_EntityId_OccurredAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_platform_AttendanceRecords_StudentId",
                table: "AttendanceRecords",
                newName: "IX_AttendanceRecords_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_AttendanceRecords_ClassSessionId_StudentId",
                table: "AttendanceRecords",
                newName: "IX_AttendanceRecords_ClassSessionId_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                newName: "IX_AspNetUserRoles_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                newName: "IX_AspNetUserLogins_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                newName: "IX_AspNetUserClaims_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                newName: "IX_AspNetRoleClaims_RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_platform_Announcements_Status_PublishedAtUtc",
                table: "Announcements",
                newName: "IX_Announcements_Status_PublishedAtUtc");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Students",
                table: "Students",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentRequirementProgress",
                table: "StudentRequirementProgress",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentRankHistory",
                table: "StudentRankHistory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RankRequirements",
                table: "RankRequirements",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Programs",
                table: "Programs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NewsPosts",
                table: "NewsPosts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MediaAssets",
                table: "MediaAssets",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuardianStudents",
                table: "GuardianStudents",
                columns: new[] { "GuardianId", "StudentId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Guardians",
                table: "Guardians",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Enrollments",
                table: "Enrollments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConsentDocuments",
                table: "ConsentDocuments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClassTemplates",
                table: "ClassTemplates",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClassSessions",
                table: "ClassSessions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BeltRanks",
                table: "BeltRanks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuditEvents",
                table: "AuditEvents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AttendanceRecords",
                table: "AttendanceRecords",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserTokens",
                table: "AspNetUserTokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUsers",
                table: "AspNetUsers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserRoles",
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserLogins",
                table: "AspNetUserLogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetUserClaims",
                table: "AspNetUserClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoles",
                table: "AspNetRoles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AspNetRoleClaims",
                table: "AspNetRoleClaims",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Announcements",
                table: "Announcements",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                table: "AspNetUserTokens",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_ClassSessions_ClassSessionId",
                table: "AttendanceRecords",
                column: "ClassSessionId",
                principalTable: "ClassSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Students_StudentId",
                table: "AttendanceRecords",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassSessions_ClassTemplates_ClassTemplateId",
                table: "ClassSessions",
                column: "ClassTemplateId",
                principalTable: "ClassTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassSessions_Students_AssignedStudentId",
                table: "ClassSessions",
                column: "AssignedStudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassTemplates_Programs_ProgramAreaId",
                table: "ClassTemplates",
                column: "ProgramAreaId",
                principalTable: "Programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentAcceptances_ConsentDocuments_ConsentDocumentId",
                table: "ConsentAcceptances",
                column: "ConsentDocumentId",
                principalTable: "ConsentDocuments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentAcceptances_Guardians_GuardianId",
                table: "ConsentAcceptances",
                column: "GuardianId",
                principalTable: "Guardians",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsentAcceptances_Students_StudentId",
                table: "ConsentAcceptances",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_ClassTemplates_ClassTemplateId",
                table: "Enrollments",
                column: "ClassTemplateId",
                principalTable: "ClassTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Students_StudentId",
                table: "Enrollments",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GuardianStudents_Guardians_GuardianId",
                table: "GuardianStudents",
                column: "GuardianId",
                principalTable: "Guardians",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GuardianStudents_Students_StudentId",
                table: "GuardianStudents",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RankRequirements_BeltRanks_BeltRankId",
                table: "RankRequirements",
                column: "BeltRankId",
                principalTable: "BeltRanks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRankHistory_BeltRanks_BeltRankId",
                table: "StudentRankHistory",
                column: "BeltRankId",
                principalTable: "BeltRanks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRankHistory_Students_StudentId",
                table: "StudentRankHistory",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRequirementProgress_RankRequirements_RankRequirementId",
                table: "StudentRequirementProgress",
                column: "RankRequirementId",
                principalTable: "RankRequirements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentRequirementProgress_Students_StudentId",
                table: "StudentRequirementProgress",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_BeltRanks_BeltRankId",
                table: "Students",
                column: "BeltRankId",
                principalTable: "BeltRanks",
                principalColumn: "Id");
        }
    }
}
