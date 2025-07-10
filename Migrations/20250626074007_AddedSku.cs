using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class AddedSku : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "CompanyId",
                table: "SKU",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateIndex(
                name: "IX_SKU_CompanyId",
                table: "SKU",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_SKU_Companies_CompanyId",
                table: "SKU",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SKU_Companies_CompanyId",
                table: "SKU");

            migrationBuilder.DropIndex(
                name: "IX_SKU_CompanyId",
                table: "SKU");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "SKU");
        }
    }
}
