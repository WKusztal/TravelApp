using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelApp.Migrations
{
    /// <inheritdoc />
    public partial class RenameArticleImageToArticleImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticleImage_Articles_ArticleId",
                table: "ArticleImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArticleImage",
                table: "ArticleImage");

            migrationBuilder.RenameTable(
                name: "ArticleImage",
                newName: "ArticleImages");

            migrationBuilder.RenameIndex(
                name: "IX_ArticleImage_ArticleId",
                table: "ArticleImages",
                newName: "IX_ArticleImages_ArticleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArticleImages",
                table: "ArticleImages",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleImages_Articles_ArticleId",
                table: "ArticleImages",
                column: "ArticleId",
                principalTable: "Articles",
                principalColumn: "ArticleId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArticleImages_Articles_ArticleId",
                table: "ArticleImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArticleImages",
                table: "ArticleImages");

            migrationBuilder.RenameTable(
                name: "ArticleImages",
                newName: "ArticleImage");

            migrationBuilder.RenameIndex(
                name: "IX_ArticleImages_ArticleId",
                table: "ArticleImage",
                newName: "IX_ArticleImage_ArticleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArticleImage",
                table: "ArticleImage",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ArticleImage_Articles_ArticleId",
                table: "ArticleImage",
                column: "ArticleId",
                principalTable: "Articles",
                principalColumn: "ArticleId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
