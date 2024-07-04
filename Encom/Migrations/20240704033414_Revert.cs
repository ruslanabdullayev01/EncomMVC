using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Encom.Migrations
{
    /// <inheritdoc />
    public partial class Revert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectProjectPhotos");

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "ProjectPhotos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPhotos_ProjectId",
                table: "ProjectPhotos",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectPhotos_Projects_ProjectId",
                table: "ProjectPhotos",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectPhotos_Projects_ProjectId",
                table: "ProjectPhotos");

            migrationBuilder.DropIndex(
                name: "IX_ProjectPhotos_ProjectId",
                table: "ProjectPhotos");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "ProjectPhotos");

            migrationBuilder.CreateTable(
                name: "ProjectProjectPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ProjectPhotoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectProjectPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectProjectPhotos_ProjectPhotos_ProjectPhotoId",
                        column: x => x.ProjectPhotoId,
                        principalTable: "ProjectPhotos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectProjectPhotos_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectProjectPhotos_ProjectId",
                table: "ProjectProjectPhotos",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectProjectPhotos_ProjectPhotoId",
                table: "ProjectProjectPhotos",
                column: "ProjectPhotoId");
        }
    }
}
