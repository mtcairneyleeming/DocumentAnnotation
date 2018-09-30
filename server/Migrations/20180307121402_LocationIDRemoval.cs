using Microsoft.EntityFrameworkCore.Migrations;

namespace DocumentAnnotation.Migrations
{
    public partial class LocationIDRemoval : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "End_LocationId",
                table: "Highlights");

            migrationBuilder.DropColumn(
                name: "Start_LocationId",
                table: "Highlights");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "End_LocationId",
                table: "Highlights",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Start_LocationId",
                table: "Highlights",
                nullable: false,
                defaultValue: 0);
        }
    }
}