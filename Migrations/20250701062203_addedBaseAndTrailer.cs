using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class addedBaseAndTrailer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bases",
                columns: table => new
                {
                    BaseId = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bases", x => x.BaseId);
                    table.ForeignKey(
                        name: "FK_Bases_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bases_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Trailers",
                columns: table => new
                {
                    TrailerId = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrailerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trailers", x => x.TrailerId);
                    table.ForeignKey(
                        name: "FK_Trailers_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trailers_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bases_BaseName",
                table: "Bases",
                column: "BaseName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bases_CreatedByUserId",
                table: "Bases",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bases_CreatedOn",
                table: "Bases",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Bases_IsActive",
                table: "Bases",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Bases_UpdatedByUserId",
                table: "Bases",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Trailers_CreatedByUserId",
                table: "Trailers",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Trailers_CreatedOn",
                table: "Trailers",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Trailers_IsActive",
                table: "Trailers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Trailers_TrailerName",
                table: "Trailers",
                column: "TrailerName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trailers_UpdatedByUserId",
                table: "Trailers",
                column: "UpdatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bases");

            migrationBuilder.DropTable(
                name: "Trailers");
        }
    }
}
