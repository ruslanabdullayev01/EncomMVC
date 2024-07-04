using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Encom.Migrations
{
    /// <inheritdoc />
    public partial class AddedPhotoGroupIdInNewsPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PhotoGroupId",
                table: "NewsPhotos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoGroupId",
                table: "NewsPhotos");
        }
    }
}
