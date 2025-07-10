using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class addedpurchasenatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "PurchaseNatureId",
                table: "ProductNatures",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateTable(
                name: "PurchaseNatures",
                columns: table => new
                {
                    PurchaseNatureId = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseNatureName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseNatures", x => x.PurchaseNatureId);
                    table.ForeignKey(
                        name: "FK_PurchaseNatures_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseNatures_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductNatures_PurchaseNatureId",
                table: "ProductNatures",
                column: "PurchaseNatureId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseNatures_CreatedByUserId",
                table: "PurchaseNatures",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseNatures_CreatedOn",
                table: "PurchaseNatures",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseNatures_UpdatedByUserId",
                table: "PurchaseNatures",
                column: "UpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductNatures_PurchaseNatures_PurchaseNatureId",
                table: "ProductNatures",
                column: "PurchaseNatureId",
                principalTable: "PurchaseNatures",
                principalColumn: "PurchaseNatureId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductNatures_PurchaseNatures_PurchaseNatureId",
                table: "ProductNatures");

            migrationBuilder.DropTable(
                name: "PurchaseNatures");

            migrationBuilder.DropIndex(
                name: "IX_ProductNatures_PurchaseNatureId",
                table: "ProductNatures");

            migrationBuilder.DropColumn(
                name: "PurchaseNatureId",
                table: "ProductNatures");
        }
    }
}
