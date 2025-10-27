using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class addUoMinGRN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "UoMId",
                table: "PurchaseOrderItems",
                type: "smallint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_UoMId",
                table: "PurchaseOrderItems",
                column: "UoMId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItems_UoMs_UoMId",
                table: "PurchaseOrderItems",
                column: "UoMId",
                principalTable: "UoMs",
                principalColumn: "UoMId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_UoMs_UoMId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderItems_UoMId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "UoMId",
                table: "PurchaseOrderItems");
        }
    }
}
