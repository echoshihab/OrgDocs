using Microsoft.EntityFrameworkCore.Migrations;

namespace OrgDocs.Migrations
{
    public partial class uploads : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PdfUrl",
                table: "Documents",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PdfUrl",
                table: "Documents");
        }
    }
}
