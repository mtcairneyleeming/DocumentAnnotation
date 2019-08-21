using Microsoft.EntityFrameworkCore.Migrations;

namespace DocumentAnnotation.Migrations
{
    public partial class SingleLocationInHighlight : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                "End_BookNumber",
                "Highlights");

            migrationBuilder.DropColumn(
                "End_GroupNumber",
                "Highlights");

            migrationBuilder.DropColumn(
                "End_SectionNumber",
                "Highlights");

            migrationBuilder.DropColumn(
                "End_WordNumber",
                "Highlights");

            migrationBuilder.RenameColumn(
                "Start_WordNumber",
                "Highlights",
                "Location_WordNumber");

            migrationBuilder.RenameColumn(
                "Start_SectionNumber",
                "Highlights",
                "Location_SectionNumber");

            migrationBuilder.RenameColumn(
                "Start_GroupNumber",
                "Highlights",
                "Location_GroupNumber");

            migrationBuilder.RenameColumn(
                "Start_BookNumber",
                "Highlights",
                "Location_BookNumber");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                "Location_WordNumber",
                "Highlights",
                "Start_WordNumber");

            migrationBuilder.RenameColumn(
                "Location_SectionNumber",
                "Highlights",
                "Start_SectionNumber");

            migrationBuilder.RenameColumn(
                "Location_GroupNumber",
                "Highlights",
                "Start_GroupNumber");

            migrationBuilder.RenameColumn(
                "Location_BookNumber",
                "Highlights",
                "Start_BookNumber");

            migrationBuilder.AddColumn<int>(
                "End_BookNumber",
                "Highlights",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                "End_GroupNumber",
                "Highlights",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                "End_SectionNumber",
                "Highlights",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                "End_WordNumber",
                "Highlights",
                nullable: false,
                defaultValue: 0);
        }
    }
}
