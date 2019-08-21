using Microsoft.EntityFrameworkCore.Migrations;

namespace DocumentAnnotation.Migrations
{
    public partial class NameChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                "FK_Annotations_DocumentAnnotations_DocumentAnnotationId",
                "Annotations");

            migrationBuilder.DropIndex(
                "IX_Annotations_DocumentAnnotationId",
                "Annotations");

            migrationBuilder.RenameColumn(
                "DocumentAnnotationId",
                "DocumentAnnotations",
                "DocumentId");

            migrationBuilder.AddColumn<string[]>(
                "AllowedUsers",
                "DocumentAnnotations",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                "Visibility",
                "DocumentAnnotations",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.RenameColumn(
                "DocumentAnnotationId",
                newName: "DocumentId",
                table: "Annotations"
                );

            migrationBuilder.CreateIndex(
                "IX_Annotations_DocumentId",
                "Annotations",
                "DocumentId");

            migrationBuilder.AddForeignKey(
                "FK_Annotations_DocumentAnnotations_DocumentId",
                "Annotations",
                "DocumentId",
                "DocumentAnnotations",
                principalColumn: "DocumentId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                "FK_Annotations_DocumentAnnotations_DocumentId",
                "Annotations");

            migrationBuilder.DropIndex(
                "IX_Annotations_DocumentId",
                "Annotations");

            migrationBuilder.DropColumn(
                "AllowedUsers",
                "DocumentAnnotations");

            migrationBuilder.DropColumn(
                "Visibility",
                "DocumentAnnotations");

            migrationBuilder.RenameColumn(
                "DocumentId",
                newName: "DocumentAnnotationId",
                table: "Annotations"
            );

            migrationBuilder.RenameColumn(
                "DocumentId",
                "DocumentAnnotations",
                "DocumentAnnotationId");

            migrationBuilder.CreateIndex(
                "IX_Annotations_DocumentAnnotationId",
                "Annotations",
                "DocumentAnnotationId");

            migrationBuilder.AddForeignKey(
                "FK_Annotations_DocumentAnnotations_DocumentAnnotationId",
                "Annotations",
                "DocumentAnnotationId",
                "DocumentAnnotations",
                principalColumn: "DocumentAnnotationId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
