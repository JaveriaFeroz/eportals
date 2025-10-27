using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class addedPaymentRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttachmentTypes",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                    AllowedSizeKB = table.Column<int>(type: "int", nullable: false),
                    StateId = table.Column<short>(type: "smallint", nullable: true),
                    PaymentNatureId = table.Column<short>(type: "smallint", nullable: true),
                    WorkFlowTypeId = table.Column<short>(type: "smallint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachmentTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttachmentTypes_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttachmentTypes_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PRQNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RequiredDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GoodsReceiptNoteId = table.Column<int>(type: "int", nullable: true),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    PayeeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PaymentModeId = table.Column<short>(type: "smallint", nullable: true),
                    PaymentTypeId = table.Column<short>(type: "smallint", nullable: true),
                    PaymentNatureId = table.Column<short>(type: "smallint", nullable: true),
                    PaymentSubNatureId = table.Column<short>(type: "smallint", nullable: true),
                    CurrencyId = table.Column<short>(type: "smallint", nullable: true),
                    SelfApplicant = table.Column<bool>(type: "bit", nullable: true),
                    PIVNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CSNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    WorkFlowTypeId = table.Column<short>(type: "smallint", nullable: false),
                    StateId = table.Column<short>(type: "smallint", nullable: false),
                    Owner = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestNatureId = table.Column<short>(type: "smallint", nullable: true),
                    RequestTypeId = table.Column<short>(type: "smallint", nullable: true),
                    DepartmentCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BranchCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentApprovalSequence = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    Approved = table.Column<bool>(type: "bit", nullable: false),
                    Rejected = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentRequests_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentRequests_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentRequests_GoodsReceiptNotes_GoodsReceiptNoteId",
                        column: x => x.GoodsReceiptNoteId,
                        principalTable: "GoodsReceiptNotes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentRequestAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentRequestId = table.Column<int>(type: "int", nullable: false),
                    AttachmentTypeId = table.Column<short>(type: "smallint", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FileContent = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    FileSizeKB = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRequestAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentRequestAttachments_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentRequestAttachments_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentRequestAttachments_AttachmentTypes_AttachmentTypeId",
                        column: x => x.AttachmentTypeId,
                        principalTable: "AttachmentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentRequestAttachments_PaymentRequests_PaymentRequestId",
                        column: x => x.PaymentRequestId,
                        principalTable: "PaymentRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentRequestCostAllocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentRequestId = table.Column<int>(type: "int", nullable: false),
                    BranchCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BranchName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DepartmentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DepartmentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Rate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRequestCostAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentRequestCostAllocations_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentRequestCostAllocations_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentRequestCostAllocations_PaymentRequests_PaymentRequestId",
                        column: x => x.PaymentRequestId,
                        principalTable: "PaymentRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentRequestDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentRequestId = table.Column<int>(type: "int", nullable: false),
                    JobNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ChargeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ChargeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    InvoiceNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PayeeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AmountExTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    STRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    OtherTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRequestDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentRequestDetails_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentRequestDetails_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentRequestDetails_PaymentRequests_PaymentRequestId",
                        column: x => x.PaymentRequestId,
                        principalTable: "PaymentRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttachmentTypes_CreatedByUserId",
                table: "AttachmentTypes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AttachmentTypes_CreatedOn",
                table: "AttachmentTypes",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_AttachmentTypes_UpdatedByUserId",
                table: "AttachmentTypes",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestAttachments_AttachmentTypeId",
                table: "PaymentRequestAttachments",
                column: "AttachmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestAttachments_CreatedByUserId",
                table: "PaymentRequestAttachments",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestAttachments_CreatedOn",
                table: "PaymentRequestAttachments",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestAttachments_PaymentRequestId",
                table: "PaymentRequestAttachments",
                column: "PaymentRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestAttachments_UpdatedByUserId",
                table: "PaymentRequestAttachments",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestCostAllocations_CreatedByUserId",
                table: "PaymentRequestCostAllocations",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestCostAllocations_CreatedOn",
                table: "PaymentRequestCostAllocations",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestCostAllocations_PaymentRequestId",
                table: "PaymentRequestCostAllocations",
                column: "PaymentRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestCostAllocations_UpdatedByUserId",
                table: "PaymentRequestCostAllocations",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestDetails_CreatedByUserId",
                table: "PaymentRequestDetails",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestDetails_CreatedOn",
                table: "PaymentRequestDetails",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestDetails_PaymentRequestId",
                table: "PaymentRequestDetails",
                column: "PaymentRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestDetails_UpdatedByUserId",
                table: "PaymentRequestDetails",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_CreatedByUserId",
                table: "PaymentRequests",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_CreatedOn",
                table: "PaymentRequests",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_GoodsReceiptNoteId",
                table: "PaymentRequests",
                column: "GoodsReceiptNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_UpdatedByUserId",
                table: "PaymentRequests",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentRequestAttachments");

            migrationBuilder.DropTable(
                name: "PaymentRequestCostAllocations");

            migrationBuilder.DropTable(
                name: "PaymentRequestDetails");

            migrationBuilder.DropTable(
                name: "AttachmentTypes");

            migrationBuilder.DropTable(
                name: "PaymentRequests");
        }
    }
}
