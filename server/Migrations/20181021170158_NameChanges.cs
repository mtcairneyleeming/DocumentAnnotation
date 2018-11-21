using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DocumentAnnotation.Migrations
{
    public partial class NameChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Annotations_DocumentAnnotations_DocumentAnnotationId",
                table: "Annotations");

            migrationBuilder.DropIndex(
                name: "IX_Annotations_DocumentAnnotationId",
                table: "Annotations");

            migrationBuilder.RenameColumn(
                name: "DocumentAnnotationId",
                table: "DocumentAnnotations",
                newName: "DocumentId");

            migrationBuilder.AddColumn<string[]>(
                name: "AllowedUsers",
                table: "DocumentAnnotations",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Visibility",
                table: "DocumentAnnotations",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.RenameColumn(
                name: "DocumentAnnotationId",
                newName: "DocumentId",
                table: "Annotations"
                );

            migrationBuilder.CreateIndex(
                name: "IX_Annotations_DocumentId",
                table: "Annotations",
                column: "DocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Annotations_DocumentAnnotations_DocumentId",
                table: "Annotations",
                column: "DocumentId",
                principalTable: "DocumentAnnotations",
                principalColumn: "DocumentId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Annotations_DocumentAnnotations_DocumentId",
                table: "Annotations");

            migrationBuilder.DropIndex(
                name: "IX_Annotations_DocumentId",
                table: "Annotations");

            migrationBuilder.DropColumn(
                name: "AllowedUsers",
                table: "DocumentAnnotations");

            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "DocumentAnnotations");

            migrationBuilder.RenameColumn(
                name: "DocumentId",
                newName: "DocumentAnnotationId",
                table: "Annotations"
            );

            migrationBuilder.RenameColumn(
                name: "DocumentId",
                table: "DocumentAnnotations",
                newName: "DocumentAnnotationId");

            migrationBuilder.CreateIndex(
                name: "IX_Annotations_DocumentAnnotationId",
                table: "Annotations",
                column: "DocumentAnnotationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Annotations_DocumentAnnotations_DocumentAnnotationId",
                table: "Annotations",
                column: "DocumentAnnotationId",
                principalTable: "DocumentAnnotations",
                principalColumn: "DocumentAnnotationId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
