using Microsoft.EntityFrameworkCore.Migrations;

namespace DocumentAnnotation.Migrations
{
    public partial class DocAnnLastLocation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastLocation_BookNumber",
                table: "DocumentAnnotations",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "LastLocation_Exact",
                table: "DocumentAnnotations",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastLocation_GroupNumber",
                table: "DocumentAnnotations",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LastLocation_SectionNumber",
                table: "DocumentAnnotations",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LastLocation_WordNumber",
                table: "DocumentAnnotations",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLocation_BookNumber",
                table: "DocumentAnnotations");

            migrationBuilder.DropColumn(
                name: "LastLocation_Exact",
                table: "DocumentAnnotations");

            migrationBuilder.DropColumn(
                name: "LastLocation_GroupNumber",
                table: "DocumentAnnotations");

            migrationBuilder.DropColumn(
                name: "LastLocation_SectionNumber",
                table: "DocumentAnnotations");

            migrationBuilder.DropColumn(
                name: "LastLocation_WordNumber",
                table: "DocumentAnnotations");
        }
    }
}
