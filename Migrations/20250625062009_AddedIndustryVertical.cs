using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class AddedIndustryVertical : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IndustryName",
                table: "IndustryVerticals",
                newName: "IndustryVerticalName");

            migrationBuilder.RenameIndex(
                name: "IX_IndustryVerticals_IndustryName",
                table: "IndustryVerticals",
                newName: "IX_IndustryVerticals_IndustryVerticalName");

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "IndustryVerticals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "IndustryVerticals",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "IndustryVerticals",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "IndustryVerticals",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IndustryVerticals_CreatedByUserId",
                table: "IndustryVerticals",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IndustryVerticals_CreatedOn",
                table: "IndustryVerticals",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_IndustryVerticals_UpdatedByUserId",
                table: "IndustryVerticals",
                column: "UpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_IndustryVerticals_AspNetUsers_CreatedByUserId",
                table: "IndustryVerticals",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IndustryVerticals_AspNetUsers_UpdatedByUserId",
                table: "IndustryVerticals",
                column: "UpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IndustryVerticals_AspNetUsers_CreatedByUserId",
                table: "IndustryVerticals");

            migrationBuilder.DropForeignKey(
                name: "FK_IndustryVerticals_AspNetUsers_UpdatedByUserId",
                table: "IndustryVerticals");

            migrationBuilder.DropIndex(
                name: "IX_IndustryVerticals_CreatedByUserId",
                table: "IndustryVerticals");

            migrationBuilder.DropIndex(
                name: "IX_IndustryVerticals_CreatedOn",
                table: "IndustryVerticals");

            migrationBuilder.DropIndex(
                name: "IX_IndustryVerticals_UpdatedByUserId",
                table: "IndustryVerticals");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "IndustryVerticals");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "IndustryVerticals");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "IndustryVerticals");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "IndustryVerticals");

            migrationBuilder.RenameColumn(
                name: "IndustryVerticalName",
                table: "IndustryVerticals",
                newName: "IndustryName");

            migrationBuilder.RenameIndex(
                name: "IX_IndustryVerticals_IndustryVerticalName",
                table: "IndustryVerticals",
                newName: "IX_IndustryVerticals_IndustryName");
        }
    }
}
