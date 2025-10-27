using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class prandgrnFormhistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GoodsReceiptNoteId",
                table: "FormHistory",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseOrderId",
                table: "FormHistory",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormHistory_GoodsReceiptNoteId",
                table: "FormHistory",
                column: "GoodsReceiptNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_FormHistory_PurchaseOrderId",
                table: "FormHistory",
                column: "PurchaseOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_FormHistory_GoodsReceiptNotes_GoodsReceiptNoteId",
                table: "FormHistory",
                column: "GoodsReceiptNoteId",
                principalTable: "GoodsReceiptNotes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FormHistory_PurchaseOrders_PurchaseOrderId",
                table: "FormHistory",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FormHistory_GoodsReceiptNotes_GoodsReceiptNoteId",
                table: "FormHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_FormHistory_PurchaseOrders_PurchaseOrderId",
                table: "FormHistory");

            migrationBuilder.DropIndex(
                name: "IX_FormHistory_GoodsReceiptNoteId",
                table: "FormHistory");

            migrationBuilder.DropIndex(
                name: "IX_FormHistory_PurchaseOrderId",
                table: "FormHistory");

            migrationBuilder.DropColumn(
                name: "GoodsReceiptNoteId",
                table: "FormHistory");

            migrationBuilder.DropColumn(
                name: "PurchaseOrderId",
                table: "FormHistory");
        }
    }
}
