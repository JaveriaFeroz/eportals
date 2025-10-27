using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedPRfields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequestDetails_PurchaseRequests_PurchaseRequestId",
                table: "PurchaseRequestDetails");

            migrationBuilder.RenameColumn(
                name: "PurchaseRequestId",
                table: "PurchaseRequestDetails",
                newName: "PRNo");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseRequestDetails_PurchaseRequestId",
                table: "PurchaseRequestDetails",
                newName: "IX_PurchaseRequestDetails_PRNo");

            migrationBuilder.AddColumn<decimal>(
                name: "GSTAmount",
                table: "PurchaseRequestDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossAmount",
                table: "PurchaseRequestDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "VATAmount",
                table: "PurchaseRequestDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequestDetails_PurchaseRequests_PRNo",
                table: "PurchaseRequestDetails",
                column: "PRNo",
                principalTable: "PurchaseRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequestDetails_PurchaseRequests_PRNo",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropColumn(
                name: "GSTAmount",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropColumn(
                name: "GrossAmount",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropColumn(
                name: "VATAmount",
                table: "PurchaseRequestDetails");

            migrationBuilder.RenameColumn(
                name: "PRNo",
                table: "PurchaseRequestDetails",
                newName: "PurchaseRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_PurchaseRequestDetails_PRNo",
                table: "PurchaseRequestDetails",
                newName: "IX_PurchaseRequestDetails_PurchaseRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequestDetails_PurchaseRequests_PurchaseRequestId",
                table: "PurchaseRequestDetails",
                column: "PurchaseRequestId",
                principalTable: "PurchaseRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
