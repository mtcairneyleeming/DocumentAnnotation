using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace DocumentAnnotation.Migrations
{
    public partial class LinkShorteners : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "LinkShorteners",
                table => new
                {
                    LinkShortenerId = table.Column<int>()
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ShortLink = table.Column<string>(),
                    OriginalLink = table.Column<string>()
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkShorteners", x => x.LinkShortenerId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "LinkShorteners");
        }
    }
}
