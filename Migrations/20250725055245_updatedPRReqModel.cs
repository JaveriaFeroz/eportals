using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class updatedPRReqModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemCode",
                table: "PurchaseRequestDetails");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "PurchaseRequestDetails");

            migrationBuilder.RenameColumn(
                name: "GrossAmount",
                table: "PurchaseRequestDetails",
                newName: "Amount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "PurchaseRequestDetails",
                newName: "GrossAmount");

            migrationBuilder.AddColumn<string>(
                name: "ItemCode",
                table: "PurchaseRequestDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "PurchaseRequestDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
