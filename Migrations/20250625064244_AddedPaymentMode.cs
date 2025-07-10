using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class AddedPaymentMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "PaymentModes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "PaymentModes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "PaymentModes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "PaymentModes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentModes_CreatedByUserId",
                table: "PaymentModes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentModes_CreatedOn",
                table: "PaymentModes",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentModes_UpdatedByUserId",
                table: "PaymentModes",
                column: "UpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentModes_AspNetUsers_CreatedByUserId",
                table: "PaymentModes",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentModes_AspNetUsers_UpdatedByUserId",
                table: "PaymentModes",
                column: "UpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentModes_AspNetUsers_CreatedByUserId",
                table: "PaymentModes");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentModes_AspNetUsers_UpdatedByUserId",
                table: "PaymentModes");

            migrationBuilder.DropIndex(
                name: "IX_PaymentModes_CreatedByUserId",
                table: "PaymentModes");

            migrationBuilder.DropIndex(
                name: "IX_PaymentModes_CreatedOn",
                table: "PaymentModes");

            migrationBuilder.DropIndex(
                name: "IX_PaymentModes_UpdatedByUserId",
                table: "PaymentModes");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "PaymentModes");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "PaymentModes");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "PaymentModes");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "PaymentModes");
        }
    }
}
