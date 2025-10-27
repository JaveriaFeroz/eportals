using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePRReqandGRN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequests_GoodsReceiptNotes_GoodsReceiptNoteId",
                table: "PaymentRequests");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRequests_GoodsReceiptNoteId",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "GoodsReceiptNoteId",
                table: "PaymentRequests");

            migrationBuilder.AddColumn<int>(
                name: "PaymentRequestId",
                table: "GoodsReceiptNotes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PaymentRequestGRNs",
                columns: table => new
                {
                    PaymentRequestId = table.Column<int>(type: "int", nullable: false),
                    GoodsReceiptNoteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRequestGRNs", x => new { x.PaymentRequestId, x.GoodsReceiptNoteId });
                    table.ForeignKey(
                        name: "FK_PaymentRequestGRNs_GoodsReceiptNotes_GoodsReceiptNoteId",
                        column: x => x.GoodsReceiptNoteId,
                        principalTable: "GoodsReceiptNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentRequestGRNs_PaymentRequests_PaymentRequestId",
                        column: x => x.PaymentRequestId,
                        principalTable: "PaymentRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptNotes_PaymentRequestId",
                table: "GoodsReceiptNotes",
                column: "PaymentRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestGRNs_GoodsReceiptNoteId",
                table: "PaymentRequestGRNs",
                column: "GoodsReceiptNoteId");


            migrationBuilder.AddForeignKey(
                name: "FK_GoodsReceiptNotes_PaymentRequests_PaymentRequestId",
                table: "GoodsReceiptNotes",
                column: "PaymentRequestId",
                principalTable: "PaymentRequests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropForeignKey(
                name: "FK_GoodsReceiptNotes_PaymentRequests_PaymentRequestId",
                table: "GoodsReceiptNotes");

            migrationBuilder.DropTable(
                name: "PaymentRequestGRNs");

            migrationBuilder.DropIndex(
                name: "IX_GoodsReceiptNotes_PaymentRequestId",
                table: "GoodsReceiptNotes");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserRoles_RoleId1",
                table: "AspNetUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserRoles_UserId1",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "PaymentRequestId",
                table: "GoodsReceiptNotes");


            migrationBuilder.AddColumn<int>(
                name: "GoodsReceiptNoteId",
                table: "PaymentRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_GoodsReceiptNoteId",
                table: "PaymentRequests",
                column: "GoodsReceiptNoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequests_GoodsReceiptNotes_GoodsReceiptNoteId",
                table: "PaymentRequests",
                column: "GoodsReceiptNoteId",
                principalTable: "GoodsReceiptNotes",
                principalColumn: "Id");
        }
    }
}
