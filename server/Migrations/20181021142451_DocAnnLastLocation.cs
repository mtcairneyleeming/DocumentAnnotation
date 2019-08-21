using Microsoft.EntityFrameworkCore.Migrations;

namespace DocumentAnnotation.Migrations
{
    public partial class DocAnnLastLocation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                "LastLocation_BookNumber",
                "DocumentAnnotations",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                "LastLocation_Exact",
                "DocumentAnnotations",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                "LastLocation_GroupNumber",
                "DocumentAnnotations",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                "LastLocation_SectionNumber",
                "DocumentAnnotations",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                "LastLocation_WordNumber",
                "DocumentAnnotations",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                "LastLocation_BookNumber",
                "DocumentAnnotations");

            migrationBuilder.DropColumn(
                "LastLocation_Exact",
                "DocumentAnnotations");

            migrationBuilder.DropColumn(
                "LastLocation_GroupNumber",
                "DocumentAnnotations");

            migrationBuilder.DropColumn(
                "LastLocation_SectionNumber",
                "DocumentAnnotations");

            migrationBuilder.DropColumn(
                "LastLocation_WordNumber",
                "DocumentAnnotations");
        }
    }
}
