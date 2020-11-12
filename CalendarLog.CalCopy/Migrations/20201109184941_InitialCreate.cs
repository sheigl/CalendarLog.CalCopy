using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CalendarLog.CalCopy.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    SettingsId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MasterTemplateFile = table.Column<string>(nullable: true),
                    ProofingFolder = table.Column<string>(nullable: true),
                    WorkingCalendarFolder = table.Column<string>(nullable: true),
                    APIKey = table.Column<string>(nullable: true),
                    APIUrl = table.Column<string>(nullable: true),
                    SecretKey = table.Column<string>(nullable: true),
                    ProoferInitials = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: true),
                    LastModifiedDate = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.SettingsId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
