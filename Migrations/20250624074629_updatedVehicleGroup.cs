using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class updatedVehicleGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_VehicleGroups",
                table: "VehicleGroups");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "VehicleGroups");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "VehicleGroups");

            migrationBuilder.RenameTable(
                name: "VehicleGroups",
                newName: "VehicleGroup");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleGroups_IsActive",
                table: "VehicleGroup",
                newName: "IX_VehicleGroup_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleGroups_GroupName",
                table: "VehicleGroup",
                newName: "IX_VehicleGroup_GroupName");

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "VehicleGroup",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "VehicleGroup",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_VehicleGroup",
                table: "VehicleGroup",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleGroup_CreatedByUserId",
                table: "VehicleGroup",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleGroup_CreatedOn",
                table: "VehicleGroup",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleGroup_UpdatedByUserId",
                table: "VehicleGroup",
                column: "UpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleGroup_AspNetUsers_CreatedByUserId",
                table: "VehicleGroup",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleGroup_AspNetUsers_UpdatedByUserId",
                table: "VehicleGroup",
                column: "UpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleGroup_AspNetUsers_CreatedByUserId",
                table: "VehicleGroup");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleGroup_AspNetUsers_UpdatedByUserId",
                table: "VehicleGroup");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VehicleGroup",
                table: "VehicleGroup");

            migrationBuilder.DropIndex(
                name: "IX_VehicleGroup_CreatedByUserId",
                table: "VehicleGroup");

            migrationBuilder.DropIndex(
                name: "IX_VehicleGroup_CreatedOn",
                table: "VehicleGroup");

            migrationBuilder.DropIndex(
                name: "IX_VehicleGroup_UpdatedByUserId",
                table: "VehicleGroup");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "VehicleGroup");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "VehicleGroup");

            migrationBuilder.RenameTable(
                name: "VehicleGroup",
                newName: "VehicleGroups");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleGroup_IsActive",
                table: "VehicleGroups",
                newName: "IX_VehicleGroups_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleGroup_GroupName",
                table: "VehicleGroups",
                newName: "IX_VehicleGroups_GroupName");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "VehicleGroups",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "VehicleGroups",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VehicleGroups",
                table: "VehicleGroups",
                column: "GroupId");
        }
    }
}
