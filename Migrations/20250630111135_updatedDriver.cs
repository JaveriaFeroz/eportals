using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class updatedDriver : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Drivers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_CompanyId",
                table: "Drivers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_ContractorId",
                table: "Drivers",
                column: "ContractorId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_QualificationId",
                table: "Drivers",
                column: "QualificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_SeparationTypeId",
                table: "Drivers",
                column: "SeparationTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Companies_CompanyId",
                table: "Drivers",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Contractors_ContractorId",
                table: "Drivers",
                column: "ContractorId",
                principalTable: "Contractors",
                principalColumn: "ContractorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Qualifications_QualificationId",
                table: "Drivers",
                column: "QualificationId",
                principalTable: "Qualifications",
                principalColumn: "QualificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_SeparationTypes_SeparationTypeId",
                table: "Drivers",
                column: "SeparationTypeId",
                principalTable: "SeparationTypes",
                principalColumn: "TypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Companies_CompanyId",
                table: "Drivers");

            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Contractors_ContractorId",
                table: "Drivers");

            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Qualifications_QualificationId",
                table: "Drivers");

            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_SeparationTypes_SeparationTypeId",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_CompanyId",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_ContractorId",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_QualificationId",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_SeparationTypeId",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Drivers");
        }
    }
}
