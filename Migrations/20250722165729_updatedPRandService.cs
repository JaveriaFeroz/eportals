using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class updatedPRandService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_PurchaseRequestItems_SourcePurchaseRequestItemId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PurchaseRequestItems",
                table: "PurchaseRequestItems");

            migrationBuilder.RenameTable(
                name: "PurchaseRequestItems",
                newName: "PurchaseRequestDetails");

            migrationBuilder.RenameColumn(
                name: "PurchasePrice",
                table: "Products",
                newName: "UnitPrice");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "PurchaseRequestDetails",
                newName: "Narration");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "PurchaseRequestDetails",
                newName: "DetailId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseRequestItems_PurchaseRequestId",
                table: "PurchaseRequestDetails",
                newName: "IX_PurchaseRequestDetails_PurchaseRequestId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "PurchaseRequestDetails",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AddColumn<decimal>(
                name: "GSTRate",
                table: "PurchaseRequestDetails",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<short>(
                name: "ProductId",
                table: "PurchaseRequestDetails",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "ServiceId",
                table: "PurchaseRequestDetails",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "UoMId",
                table: "PurchaseRequestDetails",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VATRate",
                table: "PurchaseRequestDetails",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PurchaseRequestDetails",
                table: "PurchaseRequestDetails",
                column: "DetailId");

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    ServiceId = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UnitPrice = table.Column<double>(type: "float", nullable: false),
                    UoMId = table.Column<short>(type: "smallint", nullable: true),
                    ServiceNatureId = table.Column<short>(type: "smallint", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.ServiceId);
                    table.ForeignKey(
                        name: "FK_Services_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Services_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Services_ServiceNatures_ServiceNatureId",
                        column: x => x.ServiceNatureId,
                        principalTable: "ServiceNatures",
                        principalColumn: "NatureId");
                    table.ForeignKey(
                        name: "FK_Services_UoMs_UoMId",
                        column: x => x.UoMId,
                        principalTable: "UoMs",
                        principalColumn: "UoMId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestDetails_ProductId",
                table: "PurchaseRequestDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestDetails_ServiceId",
                table: "PurchaseRequestDetails",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequestDetails_UoMId",
                table: "PurchaseRequestDetails",
                column: "UoMId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_CreatedByUserId",
                table: "Services",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_CreatedOn",
                table: "Services",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Services_ServiceNatureId",
                table: "Services",
                column: "ServiceNatureId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_UoMId",
                table: "Services",
                column: "UoMId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_UpdatedByUserId",
                table: "Services",
                column: "UpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItems_PurchaseRequestDetails_SourcePurchaseRequestItemId",
                table: "PurchaseOrderItems",
                column: "SourcePurchaseRequestItemId",
                principalTable: "PurchaseRequestDetails",
                principalColumn: "DetailId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequestDetails_Products_ProductId",
                table: "PurchaseRequestDetails",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequestDetails_PurchaseRequests_PurchaseRequestId",
                table: "PurchaseRequestDetails",
                column: "PurchaseRequestId",
                principalTable: "PurchaseRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequestDetails_Services_ServiceId",
                table: "PurchaseRequestDetails",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequestDetails_UoMs_UoMId",
                table: "PurchaseRequestDetails",
                column: "UoMId",
                principalTable: "UoMs",
                principalColumn: "UoMId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_PurchaseRequestDetails_SourcePurchaseRequestItemId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequestDetails_Products_ProductId",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequestDetails_PurchaseRequests_PurchaseRequestId",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequestDetails_Services_ServiceId",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequestDetails_UoMs_UoMId",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PurchaseRequestDetails",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequestDetails_ProductId",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequestDetails_ServiceId",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequestDetails_UoMId",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropColumn(
                name: "GSTRate",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropColumn(
                name: "UoMId",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropColumn(
                name: "VATRate",
                table: "PurchaseRequestDetails");

            migrationBuilder.RenameTable(
                name: "PurchaseRequestDetails",
                newName: "PurchaseRequestItems");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "Products",
                newName: "PurchasePrice");

            migrationBuilder.RenameColumn(
                name: "Narration",
                table: "PurchaseRequestItems",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "DetailId",
                table: "PurchaseRequestItems",
                newName: "ItemId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseRequestDetails_PurchaseRequestId",
                table: "PurchaseRequestItems",
                newName: "IX_PurchaseRequestItems_PurchaseRequestId");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "PurchaseRequestItems",
                type: "int",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PurchaseRequestItems",
                table: "PurchaseRequestItems",
                column: "ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItems_PurchaseRequestItems_SourcePurchaseRequestItemId",
                table: "PurchaseOrderItems",
                column: "SourcePurchaseRequestItemId",
                principalTable: "PurchaseRequestItems",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequestItems_PurchaseRequests_PurchaseRequestId",
                table: "PurchaseRequestItems",
                column: "PurchaseRequestId",
                principalTable: "PurchaseRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
