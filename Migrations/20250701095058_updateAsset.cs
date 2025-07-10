using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class updateAsset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Assets_AssetTypeId_IsActive",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_IsActive",
                table: "Assets");

            migrationBuilder.AlterColumn<decimal>(
                name: "StartKMs",
                table: "Assets",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "KMs",
                table: "Assets",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_BaseId",
                table: "Assets",
                column: "BaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_CapacityId",
                table: "Assets",
                column: "CapacityId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_ClientId",
                table: "Assets",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_CompanyId",
                table: "Assets",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_DriverId1",
                table: "Assets",
                column: "DriverId1");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_DriverId2",
                table: "Assets",
                column: "DriverId2");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_LeaseTypeId",
                table: "Assets",
                column: "LeaseTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_MakeId",
                table: "Assets",
                column: "MakeId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_SupplierId",
                table: "Assets",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_TrailerId",
                table: "Assets",
                column: "TrailerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Bases_BaseId",
                table: "Assets",
                column: "BaseId",
                principalTable: "Bases",
                principalColumn: "BaseId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Capacities_CapacityId",
                table: "Assets",
                column: "CapacityId",
                principalTable: "Capacities",
                principalColumn: "CapacityId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Cities_CityId",
                table: "Assets",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "CityId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Clients_ClientId",
                table: "Assets",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "ClientId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Companies_CompanyId",
                table: "Assets",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Drivers_DriverId1",
                table: "Assets",
                column: "DriverId1",
                principalTable: "Drivers",
                principalColumn: "DriverId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Drivers_DriverId2",
                table: "Assets",
                column: "DriverId2",
                principalTable: "Drivers",
                principalColumn: "DriverId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_LeaseTypes_LeaseTypeId",
                table: "Assets",
                column: "LeaseTypeId",
                principalTable: "LeaseTypes",
                principalColumn: "TypeId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Makes_MakeId",
                table: "Assets",
                column: "MakeId",
                principalTable: "Makes",
                principalColumn: "MakeId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Suppliers_SupplierId",
                table: "Assets",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "SupplierId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Trailers_TrailerId",
                table: "Assets",
                column: "TrailerId",
                principalTable: "Trailers",
                principalColumn: "TrailerId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Bases_BaseId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Capacities_CapacityId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Cities_CityId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Clients_ClientId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Companies_CompanyId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Drivers_DriverId1",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Drivers_DriverId2",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_LeaseTypes_LeaseTypeId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Makes_MakeId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Suppliers_SupplierId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Trailers_TrailerId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_BaseId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_CapacityId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_ClientId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_CompanyId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_DriverId1",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_DriverId2",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_LeaseTypeId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_MakeId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_SupplierId",
                table: "Assets");

            migrationBuilder.DropIndex(
                name: "IX_Assets_TrailerId",
                table: "Assets");

            migrationBuilder.AlterColumn<decimal>(
                name: "StartKMs",
                table: "Assets",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "KMs",
                table: "Assets",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AssetTypeId_IsActive",
                table: "Assets",
                columns: new[] { "AssetTypeId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_IsActive",
                table: "Assets",
                column: "IsActive");
        }
    }
}
