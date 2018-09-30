using Microsoft.EntityFrameworkCore.Migrations;

namespace DocumentAnnotation.Migrations
{
    public partial class Relationships : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DocumentAnnotations_TextId",
                table: "DocumentAnnotations",
                column: "TextId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAnnotations_UserId",
                table: "DocumentAnnotations",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentAnnotations_Texts_TextId",
                table: "DocumentAnnotations",
                column: "TextId",
                principalTable: "Texts",
                principalColumn: "TextId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentAnnotations_AspNetUsers_UserId",
                table: "DocumentAnnotations",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentAnnotations_Texts_TextId",
                table: "DocumentAnnotations");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentAnnotations_AspNetUsers_UserId",
                table: "DocumentAnnotations");

            migrationBuilder.DropIndex(
                name: "IX_DocumentAnnotations_TextId",
                table: "DocumentAnnotations");

            migrationBuilder.DropIndex(
                name: "IX_DocumentAnnotations_UserId",
                table: "DocumentAnnotations");
        }
    }
}