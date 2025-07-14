using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class updatedproductNature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductNatures_PurchaseNatures_PurchaseNatureId",
                table: "ProductNatures");

            migrationBuilder.DropIndex(
                name: "IX_ProductNatures_PurchaseNatureId",
                table: "ProductNatures");

            migrationBuilder.DropColumn(
                name: "PurchaseNatureId",
                table: "ProductNatures");

            migrationBuilder.AddColumn<bool>(
                name: "IsCapex",
                table: "ProductNatures",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOpex",
                table: "ProductNatures",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCapex",
                table: "ProductNatures");

            migrationBuilder.DropColumn(
                name: "IsOpex",
                table: "ProductNatures");

            migrationBuilder.AddColumn<short>(
                name: "PurchaseNatureId",
                table: "ProductNatures",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateIndex(
                name: "IX_ProductNatures_PurchaseNatureId",
                table: "ProductNatures",
                column: "PurchaseNatureId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductNatures_PurchaseNatures_PurchaseNatureId",
                table: "ProductNatures",
                column: "PurchaseNatureId",
                principalTable: "PurchaseNatures",
                principalColumn: "PurchaseNatureId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
