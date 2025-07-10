using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class AddedAuditTrails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AccessorialCharges");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "AccessorialCharges");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "AccessorialCharges",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "AccessorialCharges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "AccessorialCharges",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccessorialCharges_CreatedByUserId",
                table: "AccessorialCharges",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessorialCharges_CreatedOn",
                table: "AccessorialCharges",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_AccessorialCharges_UpdatedByUserId",
                table: "AccessorialCharges",
                column: "UpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessorialCharges_AspNetUsers_CreatedByUserId",
                table: "AccessorialCharges",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AccessorialCharges_AspNetUsers_UpdatedByUserId",
                table: "AccessorialCharges",
                column: "UpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessorialCharges_AspNetUsers_CreatedByUserId",
                table: "AccessorialCharges");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessorialCharges_AspNetUsers_UpdatedByUserId",
                table: "AccessorialCharges");

            migrationBuilder.DropIndex(
                name: "IX_AccessorialCharges_CreatedByUserId",
                table: "AccessorialCharges");

            migrationBuilder.DropIndex(
                name: "IX_AccessorialCharges_CreatedOn",
                table: "AccessorialCharges");

            migrationBuilder.DropIndex(
                name: "IX_AccessorialCharges_UpdatedByUserId",
                table: "AccessorialCharges");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "AccessorialCharges");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "AccessorialCharges");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "AccessorialCharges",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AccessorialCharges",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "AccessorialCharges",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
