using Microsoft.EntityFrameworkCore.Migrations;

namespace DocumentAnnotation.Migrations
{
    public partial class LocationIDRemoval : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                "End_LocationId",
                "Highlights");

            migrationBuilder.DropColumn(
                "Start_LocationId",
                "Highlights");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                "End_LocationId",
                "Highlights",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                "Start_LocationId",
                "Highlights",
                nullable: false,
                defaultValue: 0);
        }
    }
}