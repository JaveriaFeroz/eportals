using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class addedDriver : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Consignees_CompanyId",
                table: "Consignees",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Consignees_Companies_CompanyId",
                table: "Consignees",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SKUCategories_Companies_CompanyId",
                table: "SKUCategories",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consignees_Companies_CompanyId",
                table: "Consignees");

            migrationBuilder.DropForeignKey(
                name: "FK_SKUCategories_Companies_CompanyId",
                table: "SKUCategories");

            migrationBuilder.DropIndex(
                name: "IX_Consignees_CompanyId",
                table: "Consignees");
        }
    }
}
