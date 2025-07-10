using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class addedSeperationTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    DriverId = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<short>(type: "smallint", nullable: false),
                    DriverName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BranchId = table.Column<short>(type: "smallint", nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmployeeNo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    JoiningDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContractorId = table.Column<short>(type: "smallint", nullable: true),
                    MonthlySalary = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Experience = table.Column<decimal>(type: "numeric(3,1)", nullable: false),
                    CellNo = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    LicenseNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LicenseExpiry = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CNIC = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    CNICExpiry = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QualificationId = table.Column<short>(type: "smallint", nullable: true),
                    NoKName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NoKRelationId = table.Column<short>(type: "smallint", nullable: true),
                    PreviousEmployer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SeparationTypeId = table.Column<short>(type: "smallint", nullable: true),
                    SeparationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SeparationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.DriverId);
                    table.ForeignKey(
                        name: "FK_Drivers_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Drivers_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SeperationTypes",
                columns: table => new
                {
                    TypeId = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeperationTypes", x => x.TypeId);
                    table.ForeignKey(
                        name: "FK_SeperationTypes_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SeperationTypes_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_CreatedByUserId",
                table: "Drivers",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_CreatedOn",
                table: "Drivers",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_UpdatedByUserId",
                table: "Drivers",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SeperationTypes_CreatedByUserId",
                table: "SeperationTypes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SeperationTypes_CreatedOn",
                table: "SeperationTypes",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_SeperationTypes_IsActive",
                table: "SeperationTypes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SeperationTypes_TypeName",
                table: "SeperationTypes",
                column: "TypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeperationTypes_UpdatedByUserId",
                table: "SeperationTypes",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Drivers");

            migrationBuilder.DropTable(
                name: "SeperationTypes");
        }
    }
}
