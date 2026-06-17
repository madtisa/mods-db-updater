using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GitGudModsListLoader.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstaraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ModTitle_ModId_Title",
                table: "ModTitle");

            migrationBuilder.CreateIndex(
                name: "IX_ModTitle_ModId",
                table: "ModTitle",
                column: "ModId");

            migrationBuilder.CreateIndex(
                name: "IX_ModTitle_Title",
                table: "ModTitle",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mods_ProjectId",
                table: "Mods",
                column: "ProjectId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ModTitle_ModId",
                table: "ModTitle");

            migrationBuilder.DropIndex(
                name: "IX_ModTitle_Title",
                table: "ModTitle");

            migrationBuilder.DropIndex(
                name: "IX_Mods_ProjectId",
                table: "Mods");

            migrationBuilder.CreateIndex(
                name: "IX_ModTitle_ModId_Title",
                table: "ModTitle",
                columns: new[] { "ModId", "Title" },
                unique: true);
        }
    }
}
