using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class updatemodule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Drop all foreign keys that will be affected by our changes.
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_WorkFlowType_WorkFlowTypeId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkFlowState_WorkFlowType_WorkFlowTypeId",
                table: "WorkFlowState");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkFlowApprovalSequence_WorkFlowType_WorkFlowTypeId",
                table: "WorkFlowApprovalSequence");

            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Modules_ModuleId",
                table: "Permissions");

            // Step 2: Drop dependent indexes and primary keys before altering columns.
            migrationBuilder.DropIndex(
                name: "IX_Attachments_WorkFlowTypeId",
                table: "Attachments");

            migrationBuilder.DropIndex(
                name: "IX_WorkFlowState_WorkFlowTypeId",
                table: "WorkFlowState");

            migrationBuilder.DropIndex(
                name: "IX_WorkFlowApprovalSequence_WorkFlowTypeId",
                table: "WorkFlowApprovalSequence");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_ModuleId_Name",
                table: "Permissions");

            // NEW: Drop primary key on WorkFlowState
            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkFlowState",
                table: "WorkFlowState");

            // Step 3: Drop and re-create primary keys and columns for Modules and WorkFlowType.
            // Modules
            migrationBuilder.DropPrimaryKey(
                name: "PK_Modules",
                table: "Modules");
            migrationBuilder.DropColumn(
                name: "Id",
                table: "Modules");
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Modules",
                type: "int",
                nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");
            migrationBuilder.AddPrimaryKey(
                name: "PK_Modules",
                table: "Modules",
                column: "Id");

            // WorkFlowType
            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkFlowType",
                table: "WorkFlowType");
            migrationBuilder.DropColumn(
                name: "WorkFlowTypeId",
                table: "WorkFlowType");
            migrationBuilder.AddColumn<int>(
                name: "WorkFlowTypeId",
                table: "WorkFlowType",
                type: "int",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkFlowType",
                table: "WorkFlowType",
                column: "WorkFlowTypeId");

            // Step 4: Alter the data types of the foreign key columns in dependent tables.
            migrationBuilder.AlterColumn<int>(
                name: "WorkFlowTypeId",
                table: "WorkFlowState",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<int>(
                name: "WorkFlowTypeId",
                table: "Attachments",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<int>(
                name: "WorkFlowTypeId",
                table: "WorkFlowApprovalSequence",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<int>(
                name: "ModuleId",
                table: "Permissions",
                type: "int",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            // Step 5: Recreate primary keys on dependent tables.
            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkFlowState",
                table: "WorkFlowState",
                columns: new[] { "WorkFlowStateId", "WorkFlowTypeId" }); // Assuming this is a composite key. If not, adjust accordingly.

            // Step 6: Recreate all indexes.
            migrationBuilder.CreateIndex(
                name: "IX_Attachments_WorkFlowTypeId",
                table: "Attachments",
                column: "WorkFlowTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlowState_WorkFlowTypeId",
                table: "WorkFlowState",
                column: "WorkFlowTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlowApprovalSequence_WorkFlowTypeId",
                table: "WorkFlowApprovalSequence",
                column: "WorkFlowTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_ModuleId_Name",
                table: "Permissions",
                columns: new[] { "ModuleId", "Name" },
                unique: true);

            // Step 7: Recreate all foreign key constraints.
            migrationBuilder.AddForeignKey(
                name: "FK_WorkFlowType_Modules_WorkFlowTypeId",
                table: "WorkFlowType",
                column: "WorkFlowTypeId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_WorkFlowType_WorkFlowTypeId",
                table: "Attachments",
                column: "WorkFlowTypeId",
                principalTable: "WorkFlowType",
                principalColumn: "WorkFlowTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkFlowState_WorkFlowType_WorkFlowTypeId",
                table: "WorkFlowState",
                column: "WorkFlowTypeId",
                principalTable: "WorkFlowType",
                principalColumn: "WorkFlowTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkFlowApprovalSequence_WorkFlowType_WorkFlowTypeId",
                table: "WorkFlowApprovalSequence",
                column: "WorkFlowTypeId",
                principalTable: "WorkFlowType",
                principalColumn: "WorkFlowTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Modules_ModuleId",
                table: "Permissions",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
        // This down method is a full reverse of the up method, ensuring rollback is possible.
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Step 1: Drop all foreign keys that will be affected by the changes.
            migrationBuilder.DropForeignKey(name: "FK_WorkFlowType_Modules_WorkFlowTypeId", table: "WorkFlowType");
            migrationBuilder.DropForeignKey(name: "FK_Attachments_WorkFlowType_WorkFlowTypeId", table: "Attachments");
            migrationBuilder.DropForeignKey(name: "FK_WorkFlowState_WorkFlowType_WorkFlowTypeId", table: "WorkFlowState");
            migrationBuilder.DropForeignKey(name: "FK_WorkFlowApprovalSequence_WorkFlowType_WorkFlowTypeId", table: "WorkFlowApprovalSequence");
            migrationBuilder.DropForeignKey(name: "FK_Permissions_Modules_ModuleId", table: "Permissions");

            // Step 2: Revert the Modules table.
            migrationBuilder.DropPrimaryKey(name: "PK_Modules", table: "Modules");
            migrationBuilder.DropColumn(name: "Id", table: "Modules");
            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Modules",
                type: "int",
                nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");
            migrationBuilder.AddPrimaryKey(name: "PK_Modules", table: "Modules", column: "Id");

            // Step 3: Revert the WorkFlowType table.
            migrationBuilder.DropPrimaryKey(name: "PK_WorkFlowType", table: "WorkFlowType");
            migrationBuilder.DropColumn(name: "WorkFlowTypeId", table: "WorkFlowType");
            migrationBuilder.AddColumn<short>(
                name: "WorkFlowTypeId",
                table: "WorkFlowType",
                type: "smallint",
                nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");
            migrationBuilder.AddPrimaryKey(name: "PK_WorkFlowType", table: "WorkFlowType", column: "WorkFlowTypeId");

            // Step 4: Revert Permissions.ModuleId column.
            migrationBuilder.AlterColumn<short>(
                name: "ModuleId",
                table: "Permissions",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            // Step 5: Re-add original foreign keys.
            migrationBuilder.AddForeignKey(
                name: "FK_WorkFlowType_Modules_WorkFlowTypeId",
                table: "WorkFlowType",
                column: "WorkFlowTypeId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_WorkFlowType_WorkFlowTypeId",
                table: "Attachments",
                column: "WorkFlowTypeId",
                principalTable: "WorkFlowType",
                principalColumn: "WorkFlowTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkFlowState_WorkFlowType_WorkFlowTypeId",
                table: "WorkFlowState",
                column: "WorkFlowTypeId",
                principalTable: "WorkFlowType",
                principalColumn: "WorkFlowTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkFlowApprovalSequence_WorkFlowType_WorkFlowTypeId",
                table: "WorkFlowApprovalSequence",
                column: "WorkFlowTypeId",
                principalTable: "WorkFlowType",
                principalColumn: "WorkFlowTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Modules_ModuleId",
                table: "Permissions",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}