using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TaskKarate.Api.Data;

#nullable disable

namespace TaskKarate.Api.Data.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260926090000_StaffDojoNewsPermission")]
public partial class StaffDojoNewsPermission : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "CanPublishDojoNews",
            table: "platform_AspNetUsers",
            type: "INTEGER",
            nullable: false,
            defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CanPublishDojoNews",
            table: "platform_AspNetUsers");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        // The current model snapshot already contains this property. The migration
        // only needs the explicit schema operation above when upgrading older DBs.
    }
}
