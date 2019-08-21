using Microsoft.EntityFrameworkCore.Migrations;

namespace DocumentAnnotation.Migrations
{
    public partial class Relationships : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                "IX_DocumentAnnotations_TextId",
                "DocumentAnnotations",
                "TextId");

            migrationBuilder.CreateIndex(
                "IX_DocumentAnnotations_UserId",
                "DocumentAnnotations",
                "UserId");

            migrationBuilder.AddForeignKey(
                "FK_DocumentAnnotations_Texts_TextId",
                "DocumentAnnotations",
                "TextId",
                "Texts",
                principalColumn: "TextId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                "FK_DocumentAnnotations_AspNetUsers_UserId",
                "DocumentAnnotations",
                "UserId",
                "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                "FK_DocumentAnnotations_Texts_TextId",
                "DocumentAnnotations");

            migrationBuilder.DropForeignKey(
                "FK_DocumentAnnotations_AspNetUsers_UserId",
                "DocumentAnnotations");

            migrationBuilder.DropIndex(
                "IX_DocumentAnnotations_TextId",
                "DocumentAnnotations");

            migrationBuilder.DropIndex(
                "IX_DocumentAnnotations_UserId",
                "DocumentAnnotations");
        }
    }
}