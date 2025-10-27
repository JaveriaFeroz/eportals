using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class updatedGRNs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "OrderedQuantity",
                table: "PurchaseRequestDetails",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldDefaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "BillDCDate",
                table: "GoodsReceiptNotes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillDCNo",
                table: "GoodsReceiptNotes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "CurrencyId",
                table: "GoodsReceiptNotes",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<double>(
                name: "ExRate",
                table: "GoodsReceiptNotes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "GRNVaryFromPO",
                table: "GoodsReceiptNotes",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "GRNVaryRate",
                table: "GoodsReceiptNotes",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsImported",
                table: "GoodsReceiptNotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<short>(
                name: "PrincipalId",
                table: "GoodsReceiptNotes",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DiscRate",
                table: "GoodsReceiptNoteItems",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "GRNVaryFromPO",
                table: "GoodsReceiptNoteItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "GSTRate",
                table: "GoodsReceiptNoteItems",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "GSTonRP",
                table: "GoodsReceiptNoteItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Narration",
                table: "GoodsReceiptNoteItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "POQuantity",
                table: "GoodsReceiptNoteItems",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "RetailPrice",
                table: "GoodsReceiptNoteItems",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillDCDate",
                table: "GoodsReceiptNotes");

            migrationBuilder.DropColumn(
                name: "BillDCNo",
                table: "GoodsReceiptNotes");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "GoodsReceiptNotes");

            migrationBuilder.DropColumn(
                name: "ExRate",
                table: "GoodsReceiptNotes");

            migrationBuilder.DropColumn(
                name: "GRNVaryFromPO",
                table: "GoodsReceiptNotes");

            migrationBuilder.DropColumn(
                name: "GRNVaryRate",
                table: "GoodsReceiptNotes");

            migrationBuilder.DropColumn(
                name: "IsImported",
                table: "GoodsReceiptNotes");

            migrationBuilder.DropColumn(
                name: "PrincipalId",
                table: "GoodsReceiptNotes");

            migrationBuilder.DropColumn(
                name: "DiscRate",
                table: "GoodsReceiptNoteItems");

            migrationBuilder.DropColumn(
                name: "GRNVaryFromPO",
                table: "GoodsReceiptNoteItems");

            migrationBuilder.DropColumn(
                name: "GSTRate",
                table: "GoodsReceiptNoteItems");

            migrationBuilder.DropColumn(
                name: "GSTonRP",
                table: "GoodsReceiptNoteItems");

            migrationBuilder.DropColumn(
                name: "Narration",
                table: "GoodsReceiptNoteItems");

            migrationBuilder.DropColumn(
                name: "POQuantity",
                table: "GoodsReceiptNoteItems");

            migrationBuilder.DropColumn(
                name: "RetailPrice",
                table: "GoodsReceiptNoteItems");

            migrationBuilder.AlterColumn<decimal>(
                name: "OrderedQuantity",
                table: "PurchaseRequestDetails",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");
        }
    }
}
