using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseNatureAndItemTypesToPR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "PurchaseRequests");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RequiredDate",
                table: "PurchaseRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Owner",
                table: "PurchaseRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "PurchaseRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CompanyCode",
                table: "PurchaseRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "BranchId",
                table: "PurchaseRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<short>(
                name: "ProductNatureId",
                table: "PurchaseRequests",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseItemType",
                table: "PurchaseRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseNatureType",
                table: "PurchaseRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<short>(
                name: "ServiceNatureId",
                table: "PurchaseRequests",
                type: "smallint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_ProductNatureId",
                table: "PurchaseRequests",
                column: "ProductNatureId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_ServiceNatureId",
                table: "PurchaseRequests",
                column: "ServiceNatureId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequests_ProductNatures_ProductNatureId",
                table: "PurchaseRequests",
                column: "ProductNatureId",
                principalTable: "ProductNatures",
                principalColumn: "NatureId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequests_ServiceNatures_ServiceNatureId",
                table: "PurchaseRequests",
                column: "ServiceNatureId",
                principalTable: "ServiceNatures",
                principalColumn: "NatureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequests_ProductNatures_ProductNatureId",
                table: "PurchaseRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequests_ServiceNatures_ServiceNatureId",
                table: "PurchaseRequests");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequests_ProductNatureId",
                table: "PurchaseRequests");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequests_ServiceNatureId",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "ProductNatureId",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "PurchaseItemType",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "PurchaseNatureType",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "ServiceNatureId",
                table: "PurchaseRequests");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RequiredDate",
                table: "PurchaseRequests",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "Owner",
                table: "PurchaseRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "PurchaseRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "CompanyCode",
                table: "PurchaseRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "BranchId",
                table: "PurchaseRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "PurchaseRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
