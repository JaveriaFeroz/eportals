using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class addedbidevalutation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Procurement");

            migrationBuilder.AddColumn<int>(
                name: "BidEvaluationId",
                table: "PurchaseOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WinningBidId",
                table: "PurchaseOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "PaymentRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "PaymentRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BidEvaluations",
                schema: "Procurement",
                columns: table => new
                {
                    BidNo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BidEvaluationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PRNo = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Justification = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EvaluationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmissionDeadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstimatedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SelectedBidId = table.Column<int>(type: "int", nullable: true),
                    SelectionJustification = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    WorkFlowTypeId = table.Column<short>(type: "smallint", nullable: false),
                    StateId = table.Column<short>(type: "smallint", nullable: false),
                    Owner = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RequestNatureId = table.Column<short>(type: "smallint", nullable: true),
                    RequestTypeId = table.Column<short>(type: "smallint", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    DepartmentCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BranchCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentApprovalSequence = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    Approved = table.Column<bool>(type: "bit", nullable: false),
                    Rejected = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BidEvaluations", x => x.BidNo);
                    table.ForeignKey(
                        name: "FK_BidEvaluations_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BidEvaluations_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BidEvaluations_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "BranchId");
                    table.ForeignKey(
                        name: "FK_BidEvaluations_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "DepartmentId");
                    table.ForeignKey(
                        name: "FK_BidEvaluations_PurchaseRequests_PRNo",
                        column: x => x.PRNo,
                        principalTable: "PurchaseRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bids",
                schema: "Procurement",
                columns: table => new
                {
                    BidId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BidNo = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<short>(type: "smallint", nullable: false),
                    QuotationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SubmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeliveryDays = table.Column<int>(type: "int", nullable: false),
                    PaymentTerms = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ValidityPeriod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MeetsRequirements = table.Column<bool>(type: "bit", nullable: false),
                    IsResponsive = table.Column<bool>(type: "bit", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    QuotationDocumentPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bids", x => x.BidId);
                    table.ForeignKey(
                        name: "FK_Bids_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bids_BidEvaluations_BidNo",
                        column: x => x.BidNo,
                        principalSchema: "Procurement",
                        principalTable: "BidEvaluations",
                        principalColumn: "BidNo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bids_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BidItems",
                schema: "Procurement",
                columns: table => new
                {
                    BidItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BidId = table.Column<int>(type: "int", nullable: false),
                    PurchaseRequestItemId = table.Column<int>(type: "int", nullable: true),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Specifications = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UOM = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UoMId = table.Column<short>(type: "smallint", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MeetsSpecifications = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BidItems", x => x.BidItemId);
                    table.ForeignKey(
                        name: "FK_BidItems_Bids_BidId",
                        column: x => x.BidId,
                        principalSchema: "Procurement",
                        principalTable: "Bids",
                        principalColumn: "BidId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BidItems_PurchaseRequestDetails_PurchaseRequestItemId",
                        column: x => x.PurchaseRequestItemId,
                        principalTable: "PurchaseRequestDetails",
                        principalColumn: "DetailId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BidItems_UoMs_UoMId",
                        column: x => x.UoMId,
                        principalTable: "UoMs",
                        principalColumn: "UoMId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_BidEvaluationId",
                table: "PurchaseOrders",
                column: "BidEvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_WinningBidId",
                table: "PurchaseOrders",
                column: "WinningBidId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_BranchId",
                table: "PaymentRequests",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_DepartmentId",
                table: "PaymentRequests",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_BidEvaluations_BidEvaluationNumber",
                schema: "Procurement",
                table: "BidEvaluations",
                column: "BidEvaluationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BidEvaluations_BranchId",
                schema: "Procurement",
                table: "BidEvaluations",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BidEvaluations_CreatedByUserId",
                schema: "Procurement",
                table: "BidEvaluations",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BidEvaluations_CreatedOn",
                schema: "Procurement",
                table: "BidEvaluations",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_BidEvaluations_DepartmentId",
                schema: "Procurement",
                table: "BidEvaluations",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_BidEvaluations_PRNo",
                schema: "Procurement",
                table: "BidEvaluations",
                column: "PRNo");

            migrationBuilder.CreateIndex(
                name: "IX_BidEvaluations_SelectedBidId",
                schema: "Procurement",
                table: "BidEvaluations",
                column: "SelectedBidId");

            migrationBuilder.CreateIndex(
                name: "IX_BidEvaluations_UpdatedByUserId",
                schema: "Procurement",
                table: "BidEvaluations",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BidItems_BidId",
                schema: "Procurement",
                table: "BidItems",
                column: "BidId");

            migrationBuilder.CreateIndex(
                name: "IX_BidItems_PurchaseRequestItemId",
                schema: "Procurement",
                table: "BidItems",
                column: "PurchaseRequestItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BidItems_UoMId",
                schema: "Procurement",
                table: "BidItems",
                column: "UoMId");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_BidNo",
                schema: "Procurement",
                table: "Bids",
                column: "BidNo");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_CreatedByUserId",
                schema: "Procurement",
                table: "Bids",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_SupplierId",
                schema: "Procurement",
                table: "Bids",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequests_Branches_BranchId",
                table: "PaymentRequests",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequests_Departments_DepartmentId",
                table: "PaymentRequests",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_BidEvaluations_BidEvaluationId",
                table: "PurchaseOrders",
                column: "BidEvaluationId",
                principalSchema: "Procurement",
                principalTable: "BidEvaluations",
                principalColumn: "BidNo");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Bids_WinningBidId",
                table: "PurchaseOrders",
                column: "WinningBidId",
                principalSchema: "Procurement",
                principalTable: "Bids",
                principalColumn: "BidId");

            migrationBuilder.AddForeignKey(
                name: "FK_BidEvaluations_Bids_SelectedBidId",
                schema: "Procurement",
                table: "BidEvaluations",
                column: "SelectedBidId",
                principalSchema: "Procurement",
                principalTable: "Bids",
                principalColumn: "BidId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequests_Branches_BranchId",
                table: "PaymentRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequests_Departments_DepartmentId",
                table: "PaymentRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_BidEvaluations_BidEvaluationId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Bids_WinningBidId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_BidEvaluations_Bids_SelectedBidId",
                schema: "Procurement",
                table: "BidEvaluations");

            migrationBuilder.DropTable(
                name: "BidItems",
                schema: "Procurement");

            migrationBuilder.DropTable(
                name: "Bids",
                schema: "Procurement");

            migrationBuilder.DropTable(
                name: "BidEvaluations",
                schema: "Procurement");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_BidEvaluationId",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_WinningBidId",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRequests_BranchId",
                table: "PaymentRequests");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRequests_DepartmentId",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "BidEvaluationId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "WinningBidId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "PaymentRequests");
        }
    }
}
