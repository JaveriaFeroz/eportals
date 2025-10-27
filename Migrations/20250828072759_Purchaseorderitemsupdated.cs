using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class Purchaseorderitemsupdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceivedQuantity",
                table: "PurchaseOrderItems");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscAmount",
                table: "PurchaseOrderItems",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscRate",
                table: "PurchaseOrderItems",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GSTAmount",
                table: "PurchaseOrderItems",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GSTRate",
                table: "PurchaseOrderItems",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "PurchaseOrderItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscAmount",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "DiscRate",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "GSTAmount",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "GSTRate",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "PurchaseOrderItems");

            migrationBuilder.AddColumn<decimal>(
                name: "ReceivedQuantity",
                table: "PurchaseOrderItems",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
