using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PublishRealLiteApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIdentityUserType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "ArtistProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "ArtistProfiles");
        }
    }
}
