using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class addedSeparationTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeperationTypes");

            migrationBuilder.CreateTable(
                name: "SeparationTypes",
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
                    table.PrimaryKey("PK_SeparationTypes", x => x.TypeId);
                    table.ForeignKey(
                        name: "FK_SeparationTypes_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SeparationTypes_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeparationTypes_CreatedByUserId",
                table: "SeparationTypes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SeparationTypes_CreatedOn",
                table: "SeparationTypes",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_SeparationTypes_IsActive",
                table: "SeparationTypes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SeparationTypes_TypeName",
                table: "SeparationTypes",
                column: "TypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeparationTypes_UpdatedByUserId",
                table: "SeparationTypes",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeparationTypes");

            migrationBuilder.CreateTable(
                name: "SeperationTypes",
                columns: table => new
                {
                    TypeId = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
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
    }
}
