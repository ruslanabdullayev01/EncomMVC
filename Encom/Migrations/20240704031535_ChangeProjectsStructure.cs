using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Encom.Migrations
{
    /// <inheritdoc />
    public partial class ChangeProjectsStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectProjectPhoto");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectProjectPhotos");

            migrationBuilder.CreateTable(
                name: "ProjectProjectPhoto",
                columns: table => new
                {
                    ProjectPhotosId = table.Column<int>(type: "int", nullable: false),
                    ProjectsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectProjectPhoto", x => new { x.ProjectPhotosId, x.ProjectsId });
                    table.ForeignKey(
                        name: "FK_ProjectProjectPhoto_ProjectPhotos_ProjectPhotosId",
                        column: x => x.ProjectPhotosId,
                        principalTable: "ProjectPhotos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectProjectPhoto_Projects_ProjectsId",
                        column: x => x.ProjectsId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectProjectPhoto_ProjectsId",
                table: "ProjectProjectPhoto",
                column: "ProjectsId");
        }
    }
}
