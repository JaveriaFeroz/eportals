using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class POitemsupdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "PurchaseOrderItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseOrderItemId1",
                table: "GoodsReceiptNoteItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptNoteItems_PurchaseOrderItemId1",
                table: "GoodsReceiptNoteItems",
                column: "PurchaseOrderItemId1");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceiptNoteItems_PurchaseOrderItems_PurchaseOrderItemId1",
                table: "GoodsReceiptNoteItems",
                column: "PurchaseOrderItemId1",
                principalTable: "PurchaseOrderItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceiptNoteItems_PurchaseOrderItems_PurchaseOrderItemId1",
                table: "GoodsReceiptNoteItems");

            migrationBuilder.DropIndex(
                name: "IX_GoodsReceiptNoteItems_PurchaseOrderItemId1",
                table: "GoodsReceiptNoteItems");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderItemId1",
                table: "GoodsReceiptNoteItems");
        }
    }
}
