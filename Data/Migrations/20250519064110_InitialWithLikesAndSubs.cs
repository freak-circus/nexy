using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexy.Migrations
{
    /// <inheritdoc />
    public partial class InitialWithLikesAndSubs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<Guid>>(
                name: "RealFollowers",
                table: "ModelProfiles",
                type: "uuid[]",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LikesCount",
                table: "ModelPosts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<List<Guid>>(
                name: "RealLikes",
                table: "ModelPosts",
                type: "uuid[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RealFollowers",
                table: "ModelProfiles");

            migrationBuilder.DropColumn(
                name: "LikesCount",
                table: "ModelPosts");

            migrationBuilder.DropColumn(
                name: "RealLikes",
                table: "ModelPosts");
        }
    }
}
