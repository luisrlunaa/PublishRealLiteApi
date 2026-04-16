using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PublishRealLiteApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDecimalPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminUserId",
                table: "ArtistProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ArtistProfileId",
                table: "ArtistProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdminProfile",
                table: "ArtistProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ArtistProfiles_ArtistProfileId",
                table: "ArtistProfiles",
                column: "ArtistProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArtistProfiles_ArtistProfiles_ArtistProfileId",
                table: "ArtistProfiles",
                column: "ArtistProfileId",
                principalTable: "ArtistProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArtistProfiles_ArtistProfiles_ArtistProfileId",
                table: "ArtistProfiles");

            migrationBuilder.DropIndex(
                name: "IX_ArtistProfiles_ArtistProfileId",
                table: "ArtistProfiles");

            migrationBuilder.DropColumn(
                name: "AdminUserId",
                table: "ArtistProfiles");

            migrationBuilder.DropColumn(
                name: "ArtistProfileId",
                table: "ArtistProfiles");

            migrationBuilder.DropColumn(
                name: "IsAdminProfile",
                table: "ArtistProfiles");
        }
    }
}
