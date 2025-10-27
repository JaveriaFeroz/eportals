using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class addmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "PurchaseRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "BranchCode",
                table: "PurchaseRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyCode",
                table: "PurchaseRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "PurchaseRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentApprovalSequence",
                table: "PurchaseRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentCode",
                table: "PurchaseRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "PurchaseRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Owner",
                table: "PurchaseRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Rejected",
                table: "PurchaseRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<short>(
                name: "RequestNatureId",
                table: "PurchaseRequests",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "RequestTypeId",
                table: "PurchaseRequests",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "StateId",
                table: "PurchaseRequests",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "PurchaseRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "WorkFlowTypeId",
                table: "PurchaseRequests",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);


            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_CreatedByUserId",
                table: "PurchaseRequests",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_CreatedOn",
                table: "PurchaseRequests",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_UpdatedByUserId",
                table: "PurchaseRequests",
                column: "UpdatedByUserId");


            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequisitionDetails_CreatedByUserId",
                table: "PurchaseRequisitionDetails",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequisitionDetails_CreatedOn",
                table: "PurchaseRequisitionDetails",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequisitionDetails_PRNo",
                table: "PurchaseRequisitionDetails",
                column: "PRNo");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequisitionDetails_UpdatedByUserId",
                table: "PurchaseRequisitionDetails",
                column: "UpdatedByUserId");


            migrationBuilder.CreateIndex(
                name: "IX_WorkFlowState_WorkFlowTypeId",
                table: "WorkFlowState",
                column: "WorkFlowTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequests_AspNetUsers_CreatedByUserId",
                table: "PurchaseRequests",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequests_AspNetUsers_UpdatedByUserId",
                table: "PurchaseRequests",
                column: "UpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequests_AspNetUsers_CreatedByUserId",
                table: "PurchaseRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequests_AspNetUsers_UpdatedByUserId",
                table: "PurchaseRequests");

            migrationBuilder.DropTable(
                name: "FormHistory");

            migrationBuilder.DropTable(
                name: "PurchaseRequisitionDetails");

            migrationBuilder.DropTable(
                name: "WorkFlowApprovalSequence");

            migrationBuilder.DropTable(
                name: "WorkFlowState");

            migrationBuilder.DropTable(
                name: "WorkFlowType");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequests_CreatedByUserId",
                table: "PurchaseRequests");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequests_CreatedOn",
                table: "PurchaseRequests");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequests_UpdatedByUserId",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "Approved",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "BranchCode",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "CompanyCode",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "CurrentApprovalSequence",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "DepartmentCode",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "Owner",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "Rejected",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "RequestNatureId",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "RequestTypeId",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "StateId",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "WorkFlowTypeId",
                table: "PurchaseRequests");

            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                table: "PurchaseRequests",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "PurchaseRequests",
                newName: "CreatedAt");
        }
    }
}
