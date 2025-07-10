using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class Update_AssetStatus_DeleteBehaviorToRestrict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_AssetStatuses_StatusId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_LeaseTypes_LeaseTypeId",
                table: "Assets");

            migrationBuilder.AlterColumn<short>(
                name: "StatusId",
                table: "Assets",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AlterColumn<short>(
                name: "LeaseTypeId",
                table: "Assets",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_AssetStatuses_StatusId",
                table: "Assets",
                column: "StatusId",
                principalTable: "AssetStatuses",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_LeaseTypes_LeaseTypeId",
                table: "Assets",
                column: "LeaseTypeId",
                principalTable: "LeaseTypes",
                principalColumn: "TypeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_AssetStatuses_StatusId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_LeaseTypes_LeaseTypeId",
                table: "Assets");

            migrationBuilder.AlterColumn<short>(
                name: "StatusId",
                table: "Assets",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<short>(
                name: "LeaseTypeId",
                table: "Assets",
                type: "smallint",
                nullable: true,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_AssetStatuses_StatusId",
                table: "Assets",
                column: "StatusId",
                principalTable: "AssetStatuses",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_LeaseTypes_LeaseTypeId",
                table: "Assets",
                column: "LeaseTypeId",
                principalTable: "LeaseTypes",
                principalColumn: "TypeId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
