using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DocumentAnnotation.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentAnnotations",
                columns: table => new
                {
                    DocumentAnnotationId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    TextId = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_DocumentAnnotations", x => x.DocumentAnnotationId); });

            migrationBuilder.CreateTable(
                name: "Texts",
                columns: table => new
                {
                    TextId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Abbreviation = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Texts", x => x.TextId); });

            migrationBuilder.CreateTable(
                name: "Annotations",
                columns: table => new
                {
                    AnnotationId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Body = table.Column<string>(nullable: true),
                    DocumentAnnotationId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Annotations", x => x.AnnotationId);
                    table.ForeignKey(
                        name: "FK_Annotations_DocumentAnnotations_DocumentAnnotationId",
                        column: x => x.DocumentAnnotationId,
                        principalTable: "DocumentAnnotations",
                        principalColumn: "DocumentAnnotationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Highlights",
                columns: table => new
                {
                    HighlightId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AnnotationId = table.Column<int>(nullable: false),
                    End_BookNumber = table.Column<int>(nullable: false),
                    End_Character = table.Column<int>(nullable: false),
                    End_GroupNumber = table.Column<int>(nullable: false),
                    End_LocationId = table.Column<int>(nullable: false),
                    End_SectionNumber = table.Column<int>(nullable: false),
                    Start_BookNumber = table.Column<int>(nullable: false),
                    Start_Character = table.Column<int>(nullable: false),
                    Start_GroupNumber = table.Column<int>(nullable: false),
                    Start_LocationId = table.Column<int>(nullable: false),
                    Start_SectionNumber = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Highlights", x => x.HighlightId);
                    table.ForeignKey(
                        name: "FK_Highlights_Annotations_AnnotationId",
                        column: x => x.AnnotationId,
                        principalTable: "Annotations",
                        principalColumn: "AnnotationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Annotations_DocumentAnnotationId",
                table: "Annotations",
                column: "DocumentAnnotationId");

            migrationBuilder.CreateIndex(
                name: "IX_Highlights_AnnotationId",
                table: "Highlights",
                column: "AnnotationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Highlights");

            migrationBuilder.DropTable(
                name: "Texts");

            migrationBuilder.DropTable(
                name: "Annotations");

            migrationBuilder.DropTable(
                name: "DocumentAnnotations");
        }
    }
}