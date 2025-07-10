using ProcureToPay.Areas.Common.Services;
using ProcureToPay.Areas.Master.Services;
using ProcureToPay.Areas.Insurance.Services;
using ProcureToPay.Areas.Inventory.Services;

namespace ProcureToPay.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register all application services
            services.AddMasterServices();
     

            return services;
        }

        public static IServiceCollection AddMasterServices(this IServiceCollection services)
        {
            services.AddScoped<IAccessorialChargeService, AccessorialChargeService>();
            services.AddScoped<IActivityService, ActivityService>();
            services.AddScoped<IAssetService, AssetService>();
            services.AddScoped<IAssetTypeService, AssetTypeService>();
            services.AddScoped<IAssetStatusService, AssetStatusService>();
            services.AddScoped<IBaseService, BaseService>();
            services.AddScoped<ICityService, CityService>();
            services.AddScoped<IRegionService, RegionService>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<ICapacityService, CapacityService>();
            services.AddScoped<IChargeService, ChargeService>();
            services.AddScoped<IDriverService, DriverService>();
            services.AddScoped<IIndustryVerticalService, IndustryVerticalService>();
            services.AddScoped<IMakeService, MakeService>();
            services.AddScoped<IPaymentModeService, PaymentModeService>();
            services.AddScoped<IPurchaseNatureService, PurchaseNatureService>();
            services.AddScoped<IPriorityService, PriorityService>();
            services.AddScoped<IQualificationService, QualificationService>();
            services.AddScoped<IRateTypeService, RateTypeService>();
            services.AddScoped<IVehicleGroupService, VehicleGroupService>();
            services.AddScoped<ISeparationTypeService, SeparationTypeService>();
            services.AddScoped<ISKUService, SKUService>();
            services.AddScoped<ISKUTypeService, SKUTypeService>();
            services.AddScoped<ISKUCategoryService, SKUCategoryService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<ISupplierRateService, SupplierRateService>();
            services.AddScoped<ISupplierTypeService, SupplierTypeService>();
            services.AddScoped<ITrailerService, TrailerService>();
            services.AddScoped<IRelationService, RelationService>();
            services.AddScoped<IWorkOrderTypeService, WorkOrderTypeService>();
            services.AddScoped<IWHTaxExemptionService, WHTaxExemptionService>();
            services.AddScoped<IWarningTypeService, WarningTypeService>();
            services.AddScoped<IUoMService, UoMService>();
            services.AddScoped<ISubCategoryService, SubCategoryService>();
            services.AddScoped<IShipperService, ShipperService>();
            services.AddScoped<IProductNatureService, ProductNatureService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IProductTypeService, ProductTypeService>();
            services.AddScoped<IComplainantService, ComplainantService>();
            services.AddScoped<IConsigneeService, ConsigneeService>();
            services.AddScoped<IDetentionService, DetentionService>();
            services.AddScoped<ILeaseTypeService, LeaseTypeService>();
            services.AddScoped<IContractorService, ContractorService>();

            // Register Insurance Services
            services.AddScoped<IInsuranceTypeService, InsuranceTypeService>();
            services.AddScoped<IInsuranceDocumentTypeService, InsuranceDocumentTypeService>();
            services.AddScoped<IInsuranceCompanyService, InsuranceCompanyService>();

            //Register Inventory Services
            services.AddScoped<IPurchaseRequisitionService, PurchaseRequisitionService>();

            return services;
        }

       

    }
}