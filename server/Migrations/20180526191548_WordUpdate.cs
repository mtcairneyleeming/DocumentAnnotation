using Microsoft.EntityFrameworkCore.Migrations;

namespace DocumentAnnotation.Migrations
{
    public partial class WordUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                "Start_Character",
                "Highlights",
                "Start_WordNumber");

            migrationBuilder.RenameColumn(
                "End_Character",
                "Highlights",
                "End_WordNumber");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                "Start_WordNumber",
                "Highlights",
                "Start_Character");

            migrationBuilder.RenameColumn(
                "End_WordNumber",
                "Highlights",
                "End_Character");
        }
    }
}