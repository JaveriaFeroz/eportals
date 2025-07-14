using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class addedservicesnature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequisitions_PurchaseNatures_PurchaseNatureId",
                table: "PurchaseRequisitions");

            migrationBuilder.DropTable(
                name: "PurchaseNatures");

            migrationBuilder.RenameColumn(
                name: "PurchaseNatureId",
                table: "PurchaseRequisitions",
                newName: "ServiceNatureId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseRequisitions_PurchaseNatureId",
                table: "PurchaseRequisitions",
                newName: "IX_PurchaseRequisitions_ServiceNatureId");

            migrationBuilder.CreateTable(
                name: "ServiceNatures",
                columns: table => new
                {
                    NatureId = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NatureName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsOpex = table.Column<bool>(type: "bit", nullable: false),
                    IsCapex = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceNatures", x => x.NatureId);
                    table.ForeignKey(
                        name: "FK_ServiceNatures_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceNatures_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceNatures_CreatedByUserId",
                table: "ServiceNatures",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceNatures_CreatedOn",
                table: "ServiceNatures",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceNatures_UpdatedByUserId",
                table: "ServiceNatures",
                column: "UpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequisitions_ServiceNatures_ServiceNatureId",
                table: "PurchaseRequisitions",
                column: "ServiceNatureId",
                principalTable: "ServiceNatures",
                principalColumn: "NatureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequisitions_ServiceNatures_ServiceNatureId",
                table: "PurchaseRequisitions");

            migrationBuilder.DropTable(
                name: "ServiceNatures");

            migrationBuilder.RenameColumn(
                name: "ServiceNatureId",
                table: "PurchaseRequisitions",
                newName: "PurchaseNatureId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseRequisitions_ServiceNatureId",
                table: "PurchaseRequisitions",
                newName: "IX_PurchaseRequisitions_PurchaseNatureId");

            migrationBuilder.CreateTable(
                name: "PurchaseNatures",
                columns: table => new
                {
                    PurchaseNatureId = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PurchaseNatureName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
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
                name: "FK_PurchaseRequisitions_PurchaseNatures_PurchaseNatureId",
                table: "PurchaseRequisitions",
                column: "PurchaseNatureId",
                principalTable: "PurchaseNatures",
                principalColumn: "PurchaseNatureId");
        }
    }
}
