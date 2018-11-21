using Microsoft.EntityFrameworkCore.Migrations;

namespace DocumentAnnotation.Migrations
{
    public partial class AddInexactAnnotations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Location_Exact",
                table: "Highlights",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location_Exact",
                table: "Highlights");
        }
    }
}
