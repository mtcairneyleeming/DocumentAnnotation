using Microsoft.EntityFrameworkCore.Migrations;

namespace DocumentAnnotation.Migrations
{
    public partial class AddNewLineToDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                "AddNewLine",
                "Texts",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                "AddNewLine",
                "Texts");
        }
    }
}
