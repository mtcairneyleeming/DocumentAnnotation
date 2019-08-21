using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DocumentAnnotation.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "DocumentAnnotations",
                table => new
                {
                    DocumentAnnotationId = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    TextId = table.Column<int>(),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_DocumentAnnotations", x => x.DocumentAnnotationId); });

            migrationBuilder.CreateTable(
                "Texts",
                table => new
                {
                    TextId = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Abbreviation = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Texts", x => x.TextId); });

            migrationBuilder.CreateTable(
                "Annotations",
                table => new
                {
                    AnnotationId = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Body = table.Column<string>(nullable: true),
                    DocumentAnnotationId = table.Column<int>(),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Annotations", x => x.AnnotationId);
                    table.ForeignKey(
                        "FK_Annotations_DocumentAnnotations_DocumentAnnotationId",
                        x => x.DocumentAnnotationId,
                        "DocumentAnnotations",
                        "DocumentAnnotationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "Highlights",
                table => new
                {
                    HighlightId = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AnnotationId = table.Column<int>(),
                    End_BookNumber = table.Column<int>(),
                    End_Character = table.Column<int>(),
                    End_GroupNumber = table.Column<int>(),
                    End_LocationId = table.Column<int>(),
                    End_SectionNumber = table.Column<int>(),
                    Start_BookNumber = table.Column<int>(),
                    Start_Character = table.Column<int>(),
                    Start_GroupNumber = table.Column<int>(),
                    Start_LocationId = table.Column<int>(),
                    Start_SectionNumber = table.Column<int>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Highlights", x => x.HighlightId);
                    table.ForeignKey(
                        "FK_Highlights_Annotations_AnnotationId",
                        x => x.AnnotationId,
                        "Annotations",
                        "AnnotationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                "IX_Annotations_DocumentAnnotationId",
                "Annotations",
                "DocumentAnnotationId");

            migrationBuilder.CreateIndex(
                "IX_Highlights_AnnotationId",
                "Highlights",
                "AnnotationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "Highlights");

            migrationBuilder.DropTable(
                "Texts");

            migrationBuilder.DropTable(
                "Annotations");

            migrationBuilder.DropTable(
                "DocumentAnnotations");
        }
    }
}