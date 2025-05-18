using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexy.Migrations
{
    /// <inheritdoc />
    public partial class InitialWithOnlyLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OnlyLink",
                table: "ModelProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnlyLink",
                table: "ModelProfiles");
        }
    }
}
