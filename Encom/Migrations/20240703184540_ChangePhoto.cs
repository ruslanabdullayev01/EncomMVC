using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Encom.Migrations
{
    /// <inheritdoc />
    public partial class ChangePhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "PhotoGroupId",
                table: "NewsPhotos");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectProjectPhoto");

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "ProjectPhotos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PhotoGroupId",
                table: "NewsPhotos",
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
    }
}
