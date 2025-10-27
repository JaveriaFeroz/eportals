using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class updatedPRItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "PurchaseRequestDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "PurchaseRequestDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "PurchaseRequestDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "PurchaseRequestDetails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestDetails_CreatedByUserId",
                table: "PurchaseRequestDetails",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestDetails_CreatedOn",
                table: "PurchaseRequestDetails",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestDetails_UpdatedByUserId",
                table: "PurchaseRequestDetails",
                column: "UpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequestDetails_AspNetUsers_CreatedByUserId",
                table: "PurchaseRequestDetails",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequestDetails_AspNetUsers_UpdatedByUserId",
                table: "PurchaseRequestDetails",
                column: "UpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequestDetails_AspNetUsers_CreatedByUserId",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequestDetails_AspNetUsers_UpdatedByUserId",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequestDetails_CreatedByUserId",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequestDetails_CreatedOn",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequestDetails_UpdatedByUserId",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "PurchaseRequestDetails");
        }
    }
}
