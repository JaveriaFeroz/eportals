using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcureToPay.Migrations
{
    /// <inheritdoc />
    public partial class addedremainingmodels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TypeName",
                table: "RateTypes",
                newName: "RateTypeName");

            migrationBuilder.RenameColumn(
                name: "TypeId",
                table: "RateTypes",
                newName: "RateTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_RateTypes_TypeName",
                table: "RateTypes",
                newName: "IX_RateTypes_RateTypeName");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "VehicleGroups",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "DepartmentCode",
                table: "Departments",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Charges",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Capacities",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "BranchCode",
                table: "Branches",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    CompanyId = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CompanyAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NTN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PeriodName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DistanceThreshold = table.Column<short>(type: "smallint", nullable: true),
                    ReportGraceHRs = table.Column<short>(type: "smallint", nullable: true),
                    BankAccountId = table.Column<short>(type: "smallint", nullable: true),
                    ARPeriodId = table.Column<short>(type: "smallint", nullable: true),
                    ARAccountId = table.Column<short>(type: "smallint", nullable: true),
                    APPeriodId = table.Column<short>(type: "smallint", nullable: true),
                    APAccountId = table.Column<short>(type: "smallint", nullable: true),
                    GLPeriodId = table.Column<short>(type: "smallint", nullable: true),
                    OpsPeriodId = table.Column<short>(type: "smallint", nullable: true),
                    TripRevenueAccountId = table.Column<short>(type: "smallint", nullable: true),
                    FuelExpenseAccountId = table.Column<short>(type: "smallint", nullable: true),
                    AdvanceAccountId = table.Column<short>(type: "smallint", nullable: true),
                    EnableGL = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EnablePartialDelivery = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RouteByConsignee = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    SeparateFixedInvoice = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsMandatoryDriver2 = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AllowTrailer = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.CompanyId);
                    table.ForeignKey(
                        name: "FK_Companies_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Companies_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IndustryVerticals",
                columns: table => new
                {
                    IndustryVerticalId = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IndustryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndustryVerticals", x => x.IndustryVerticalId);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceFormats",
                columns: table => new
                {
                    FormatId = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FormatName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceFormats", x => x.FormatId);
                });

            migrationBuilder.CreateTable(
                name: "PaymentModes",
                columns: table => new
                {
                    PaymentModeId = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentModeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentModes", x => x.PaymentModeId);
                });

            migrationBuilder.CreateTable(
                name: "SKUCategories",
                columns: table => new
                {
                    CategoryId = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CompanyId = table.Column<short>(type: "smallint", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SKUCategories", x => x.CategoryId);
                    table.ForeignKey(
                        name: "FK_SKUCategories_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SKUCategories_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SKUTypes",
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
                    table.PrimaryKey("PK_SKUTypes", x => x.TypeId);
                    table.ForeignKey(
                        name: "FK_SKUTypes_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SKUTypes_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubCategories",
                columns: table => new
                {
                    SubCategoryId = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubCategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategories", x => x.SubCategoryId);
                    table.ForeignKey(
                        name: "FK_SubCategories_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubCategories_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UoMs",
                columns: table => new
                {
                    UoMId = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UoMName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UoMs", x => x.UoMId);
                    table.ForeignKey(
                        name: "FK_UoMs_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UoMs_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WarningTypes",
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
                    table.PrimaryKey("PK_WarningTypes", x => x.TypeId);
                    table.ForeignKey(
                        name: "FK_WarningTypes_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarningTypes_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WHTaxExemptions",
                columns: table => new
                {
                    ExemptionId = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateFrom = table.Column<DateTime>(type: "datetime", nullable: false),
                    DateTo = table.Column<DateTime>(type: "datetime", nullable: false),
                    CompanyId = table.Column<short>(type: "smallint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WHTaxExemptions", x => x.ExemptionId);
                    table.ForeignKey(
                        name: "FK_WHTaxExemptions_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WHTaxExemptions_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderTypes",
                columns: table => new
                {
                    WorkOrderTypeId = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WorkOrderTypeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    WorkFlowId = table.Column<short>(type: "smallint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderTypes", x => x.WorkOrderTypeId);
                    table.ForeignKey(
                        name: "FK_WorkOrderTypes_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrderTypes_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    ClientId = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<short>(type: "smallint", nullable: true),
                    ClientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CityId = table.Column<short>(type: "smallint", nullable: true),
                    IndustryVerticalId = table.Column<short>(type: "smallint", nullable: true),
                    ContractPeriod = table.Column<short>(type: "smallint", nullable: true),
                    ContactNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    URL = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaymentModeId = table.Column<short>(type: "smallint", nullable: true),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreditDays = table.Column<short>(type: "smallint", nullable: true),
                    RateTypeId = table.Column<short>(type: "smallint", nullable: true),
                    CWClientId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NTN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    STRN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CategoryMandatory = table.Column<bool>(type: "bit", nullable: false),
                    ProductMandatory = table.Column<bool>(type: "bit", nullable: false),
                    DetGraceHRs = table.Column<short>(type: "smallint", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CompanyId = table.Column<short>(type: "smallint", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.ClientId);
                    table.ForeignKey(
                        name: "FK_Clients_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Clients_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Clients_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "CityId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Clients_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Clients_IndustryVerticals_IndustryVerticalId",
                        column: x => x.IndustryVerticalId,
                        principalTable: "IndustryVerticals",
                        principalColumn: "IndustryVerticalId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Clients_PaymentModes_PaymentModeId",
                        column: x => x.PaymentModeId,
                        principalTable: "PaymentModes",
                        principalColumn: "PaymentModeId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Clients_RateTypes_RateTypeId",
                        column: x => x.RateTypeId,
                        principalTable: "RateTypes",
                        principalColumn: "RateTypeId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SKUCategoryClients",
                columns: table => new
                {
                    DetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<short>(type: "smallint", nullable: false),
                    ClientId = table.Column<short>(type: "smallint", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SKUCategoryClients", x => x.DetailId);
                    table.ForeignKey(
                        name: "FK_SKUCategoryClients_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SKUCategoryClients_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SKUCategoryClients_SKUCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "SKUCategories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SKU",
                columns: table => new
                {
                    SKUId = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SKUName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SKUTypeId = table.Column<short>(type: "smallint", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SKU", x => x.SKUId);
                    table.ForeignKey(
                        name: "FK_SKU_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SKU_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SKU_SKUTypes_SKUTypeId",
                        column: x => x.SKUTypeId,
                        principalTable: "SKUTypes",
                        principalColumn: "TypeId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ClientInvoiceFormats",
                columns: table => new
                {
                    DetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<short>(type: "smallint", nullable: false),
                    FormatId = table.Column<short>(type: "smallint", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientInvoiceFormats", x => x.DetailId);
                    table.ForeignKey(
                        name: "FK_ClientInvoiceFormats_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientInvoiceFormats_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientInvoiceFormats_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientInvoiceFormats_InvoiceFormats_FormatId",
                        column: x => x.FormatId,
                        principalTable: "InvoiceFormats",
                        principalColumn: "FormatId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Shippers",
                columns: table => new
                {
                    ShipperId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShipperName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CityId = table.Column<short>(type: "smallint", nullable: true),
                    ClientId = table.Column<short>(type: "smallint", nullable: true),
                    ContactNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CompanyId = table.Column<short>(type: "smallint", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shippers", x => x.ShipperId);
                    table.ForeignKey(
                        name: "FK_Shippers_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Shippers_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Shippers_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "CityId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Shippers_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Shippers_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SKUClient",
                columns: table => new
                {
                    DetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SKUId = table.Column<short>(type: "smallint", nullable: false),
                    ClientId = table.Column<short>(type: "smallint", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SKUClient", x => x.DetailId);
                    table.ForeignKey(
                        name: "FK_SKUClient_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SKUClient_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SKUClient_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SKUClient_SKU_SKUId",
                        column: x => x.SKUId,
                        principalTable: "SKU",
                        principalColumn: "SKUId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleGroups_GroupName",
                table: "VehicleGroups",
                column: "GroupName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleGroups_IsActive",
                table: "VehicleGroups",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Charges_ChargeName",
                table: "Charges",
                column: "ChargeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Charges_IsActive",
                table: "Charges",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Capacities_CapacityName",
                table: "Capacities",
                column: "CapacityName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Capacities_IsActive",
                table: "Capacities",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ClientInvoiceFormats_ClientId",
                table: "ClientInvoiceFormats",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientInvoiceFormats_ClientId_FormatId",
                table: "ClientInvoiceFormats",
                columns: new[] { "ClientId", "FormatId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientInvoiceFormats_CreatedByUserId",
                table: "ClientInvoiceFormats",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientInvoiceFormats_CreatedOn",
                table: "ClientInvoiceFormats",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_ClientInvoiceFormats_FormatId",
                table: "ClientInvoiceFormats",
                column: "FormatId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientInvoiceFormats_UpdatedByUserId",
                table: "ClientInvoiceFormats",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CityId",
                table: "Clients",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_ClientName",
                table: "Clients",
                column: "ClientName");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CompanyId",
                table: "Clients",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CompanyId_IsActive",
                table: "Clients",
                columns: new[] { "CompanyId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CreatedByUserId",
                table: "Clients",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CreatedOn",
                table: "Clients",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Email",
                table: "Clients",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_IndustryVerticalId",
                table: "Clients",
                column: "IndustryVerticalId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_IsActive",
                table: "Clients",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_NTN",
                table: "Clients",
                column: "NTN");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_PaymentModeId",
                table: "Clients",
                column: "PaymentModeId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_RateTypeId",
                table: "Clients",
                column: "RateTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_ShortName",
                table: "Clients",
                column: "ShortName");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_UpdatedByUserId",
                table: "Clients",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_CompanyName",
                table: "Companies",
                column: "CompanyName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_CreatedByUserId",
                table: "Companies",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_CreatedOn",
                table: "Companies",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_IsActive",
                table: "Companies",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_NTN",
                table: "Companies",
                column: "NTN");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_UpdatedByUserId",
                table: "Companies",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IndustryVerticals_IndustryName",
                table: "IndustryVerticals",
                column: "IndustryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IndustryVerticals_IsActive",
                table: "IndustryVerticals",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceFormats_FormatName",
                table: "InvoiceFormats",
                column: "FormatName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceFormats_IsActive",
                table: "InvoiceFormats",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentModes_IsActive",
                table: "PaymentModes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentModes_PaymentModeName",
                table: "PaymentModes",
                column: "PaymentModeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shippers_CityId",
                table: "Shippers",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Shippers_ClientId",
                table: "Shippers",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Shippers_CompanyId",
                table: "Shippers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Shippers_CompanyId_IsActive",
                table: "Shippers",
                columns: new[] { "CompanyId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Shippers_CreatedByUserId",
                table: "Shippers",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Shippers_CreatedOn",
                table: "Shippers",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_Shippers_IsActive",
                table: "Shippers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Shippers_ShipperName",
                table: "Shippers",
                column: "ShipperName");

            migrationBuilder.CreateIndex(
                name: "IX_Shippers_UpdatedByUserId",
                table: "Shippers",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SKU_CreatedByUserId",
                table: "SKU",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SKU_CreatedOn",
                table: "SKU",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_SKU_IsActive",
                table: "SKU",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SKU_SKUName",
                table: "SKU",
                column: "SKUName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SKU_SKUTypeId",
                table: "SKU",
                column: "SKUTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SKU_SKUTypeId_IsActive",
                table: "SKU",
                columns: new[] { "SKUTypeId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SKU_UpdatedByUserId",
                table: "SKU",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SKUCategories_CategoryName",
                table: "SKUCategories",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SKUCategories_CompanyId",
                table: "SKUCategories",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SKUCategories_CompanyId_IsActive",
                table: "SKUCategories",
                columns: new[] { "CompanyId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SKUCategories_CreatedByUserId",
                table: "SKUCategories",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SKUCategories_CreatedOn",
                table: "SKUCategories",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_SKUCategories_IsActive",
                table: "SKUCategories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SKUCategories_UpdatedByUserId",
                table: "SKUCategories",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SKUCategoryClients_CategoryId",
                table: "SKUCategoryClients",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SKUCategoryClients_CategoryId_ClientId",
                table: "SKUCategoryClients",
                columns: new[] { "CategoryId", "ClientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SKUCategoryClients_ClientId",
                table: "SKUCategoryClients",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_SKUCategoryClients_CreatedByUserId",
                table: "SKUCategoryClients",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SKUCategoryClients_CreatedOn",
                table: "SKUCategoryClients",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_SKUCategoryClients_UpdatedByUserId",
                table: "SKUCategoryClients",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SKUClient_ClientId",
                table: "SKUClient",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_SKUClient_CreatedByUserId",
                table: "SKUClient",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SKUClient_CreatedOn",
                table: "SKUClient",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_SKUClient_SKUId",
                table: "SKUClient",
                column: "SKUId");

            migrationBuilder.CreateIndex(
                name: "IX_SKUClient_SKUId_ClientId",
                table: "SKUClient",
                columns: new[] { "SKUId", "ClientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SKUClient_UpdatedByUserId",
                table: "SKUClient",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SKUTypes_CreatedByUserId",
                table: "SKUTypes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SKUTypes_CreatedOn",
                table: "SKUTypes",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_SKUTypes_IsActive",
                table: "SKUTypes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SKUTypes_TypeName",
                table: "SKUTypes",
                column: "TypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SKUTypes_UpdatedByUserId",
                table: "SKUTypes",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategories_CreatedByUserId",
                table: "SubCategories",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategories_CreatedOn",
                table: "SubCategories",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategories_IsActive",
                table: "SubCategories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategories_SubCategoryName",
                table: "SubCategories",
                column: "SubCategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubCategories_UpdatedByUserId",
                table: "SubCategories",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UoMs_CreatedByUserId",
                table: "UoMs",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UoMs_CreatedOn",
                table: "UoMs",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_UoMs_IsActive",
                table: "UoMs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UoMs_UoMName",
                table: "UoMs",
                column: "UoMName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UoMs_UpdatedByUserId",
                table: "UoMs",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WarningTypes_CreatedByUserId",
                table: "WarningTypes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WarningTypes_CreatedOn",
                table: "WarningTypes",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_WarningTypes_IsActive",
                table: "WarningTypes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WarningTypes_TypeName",
                table: "WarningTypes",
                column: "TypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarningTypes_UpdatedByUserId",
                table: "WarningTypes",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WHTaxExemptions_CompanyId",
                table: "WHTaxExemptions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_WHTaxExemptions_CompanyId_DateFrom_DateTo",
                table: "WHTaxExemptions",
                columns: new[] { "CompanyId", "DateFrom", "DateTo" });

            migrationBuilder.CreateIndex(
                name: "IX_WHTaxExemptions_CreatedByUserId",
                table: "WHTaxExemptions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WHTaxExemptions_CreatedOn",
                table: "WHTaxExemptions",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_WHTaxExemptions_DateFrom",
                table: "WHTaxExemptions",
                column: "DateFrom");

            migrationBuilder.CreateIndex(
                name: "IX_WHTaxExemptions_DateTo",
                table: "WHTaxExemptions",
                column: "DateTo");

            migrationBuilder.CreateIndex(
                name: "IX_WHTaxExemptions_IsActive",
                table: "WHTaxExemptions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WHTaxExemptions_UpdatedByUserId",
                table: "WHTaxExemptions",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderTypes_CreatedByUserId",
                table: "WorkOrderTypes",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderTypes_CreatedOn",
                table: "WorkOrderTypes",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderTypes_DisplayOrder",
                table: "WorkOrderTypes",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderTypes_IsActive",
                table: "WorkOrderTypes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderTypes_UpdatedByUserId",
                table: "WorkOrderTypes",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderTypes_WorkFlowId",
                table: "WorkOrderTypes",
                column: "WorkFlowId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderTypes_WorkOrderTypeCode",
                table: "WorkOrderTypes",
                column: "WorkOrderTypeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderTypes_WorkOrderTypeName",
                table: "WorkOrderTypes",
                column: "WorkOrderTypeName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientInvoiceFormats");

            migrationBuilder.DropTable(
                name: "Shippers");

            migrationBuilder.DropTable(
                name: "SKUCategoryClients");

            migrationBuilder.DropTable(
                name: "SKUClient");

            migrationBuilder.DropTable(
                name: "SubCategories");

            migrationBuilder.DropTable(
                name: "UoMs");

            migrationBuilder.DropTable(
                name: "WarningTypes");

            migrationBuilder.DropTable(
                name: "WHTaxExemptions");

            migrationBuilder.DropTable(
                name: "WorkOrderTypes");

            migrationBuilder.DropTable(
                name: "InvoiceFormats");

            migrationBuilder.DropTable(
                name: "SKUCategories");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "SKU");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "IndustryVerticals");

            migrationBuilder.DropTable(
                name: "PaymentModes");

            migrationBuilder.DropTable(
                name: "SKUTypes");

            migrationBuilder.DropIndex(
                name: "IX_VehicleGroups_GroupName",
                table: "VehicleGroups");

            migrationBuilder.DropIndex(
                name: "IX_VehicleGroups_IsActive",
                table: "VehicleGroups");

            migrationBuilder.DropIndex(
                name: "IX_Charges_ChargeName",
                table: "Charges");

            migrationBuilder.DropIndex(
                name: "IX_Charges_IsActive",
                table: "Charges");

            migrationBuilder.DropIndex(
                name: "IX_Capacities_CapacityName",
                table: "Capacities");

            migrationBuilder.DropIndex(
                name: "IX_Capacities_IsActive",
                table: "Capacities");

            migrationBuilder.RenameColumn(
                name: "RateTypeName",
                table: "RateTypes",
                newName: "TypeName");

            migrationBuilder.RenameColumn(
                name: "RateTypeId",
                table: "RateTypes",
                newName: "TypeId");

            migrationBuilder.RenameIndex(
                name: "IX_RateTypes_RateTypeName",
                table: "RateTypes",
                newName: "IX_RateTypes_TypeName");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "VehicleGroups",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "DepartmentCode",
                table: "Departments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Charges",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Capacities",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "BranchCode",
                table: "Branches",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);
        }
    }
}
