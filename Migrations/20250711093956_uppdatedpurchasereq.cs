using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class uppdatedpurchasereq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequisitions_Branches_BranchId",
                table: "PurchaseRequisitions");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequisitions_Departments_DepartmentId",
                table: "PurchaseRequisitions");

            migrationBuilder.DropColumn(
                name: "ProductGroupId",
                table: "PurchaseRequisitions");

            migrationBuilder.RenameColumn(
                name: "ServiceGroupId",
                table: "PurchaseRequisitions",
                newName: "PurchaseNatureId");

            migrationBuilder.RenameColumn(
                name: "RequestNatureId",
                table: "PurchaseRequisitions",
                newName: "ProductNatureId");

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "PurchaseRequisitions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BranchId",
                table: "PurchaseRequisitions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequisitions_ProductNatureId",
                table: "PurchaseRequisitions",
                column: "ProductNatureId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequisitions_PurchaseNatureId",
                table: "PurchaseRequisitions",
                column: "PurchaseNatureId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequisitions_Branches_BranchId",
                table: "PurchaseRequisitions",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequisitions_Departments_DepartmentId",
                table: "PurchaseRequisitions",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequisitions_ProductNatures_ProductNatureId",
                table: "PurchaseRequisitions",
                column: "ProductNatureId",
                principalTable: "ProductNatures",
                principalColumn: "NatureId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequisitions_PurchaseNatures_PurchaseNatureId",
                table: "PurchaseRequisitions",
                column: "PurchaseNatureId",
                principalTable: "PurchaseNatures",
                principalColumn: "PurchaseNatureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequisitions_Branches_BranchId",
                table: "PurchaseRequisitions");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequisitions_Departments_DepartmentId",
                table: "PurchaseRequisitions");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequisitions_ProductNatures_ProductNatureId",
                table: "PurchaseRequisitions");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequisitions_PurchaseNatures_PurchaseNatureId",
                table: "PurchaseRequisitions");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequisitions_ProductNatureId",
                table: "PurchaseRequisitions");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequisitions_PurchaseNatureId",
                table: "PurchaseRequisitions");

            migrationBuilder.RenameColumn(
                name: "PurchaseNatureId",
                table: "PurchaseRequisitions",
                newName: "ServiceGroupId");

            migrationBuilder.RenameColumn(
                name: "ProductNatureId",
                table: "PurchaseRequisitions",
                newName: "RequestNatureId");

            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "PurchaseRequisitions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "BranchId",
                table: "PurchaseRequisitions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<short>(
                name: "ProductGroupId",
                table: "PurchaseRequisitions",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequisitions_Branches_BranchId",
                table: "PurchaseRequisitions",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequisitions_Departments_DepartmentId",
                table: "PurchaseRequisitions",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId");
        }
    }
}
