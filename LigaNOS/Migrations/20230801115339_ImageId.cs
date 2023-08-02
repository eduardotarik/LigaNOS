using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LigaNOS.Migrations
{
    public partial class ImageId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Emblem",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "Picture",
                table: "Players");

            migrationBuilder.AddColumn<Guid>(
                name: "ImageId",
                table: "Teams",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PictureId",
                table: "Players",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "PictureId",
                table: "Players");

            migrationBuilder.AddColumn<string>(
                name: "Emblem",
                table: "Teams",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Picture",
                table: "Players",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
