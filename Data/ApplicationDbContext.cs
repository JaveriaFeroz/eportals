using Microsoft.AspNetCore.Http; // For IHttpContextAccessor
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProcureToPay.Areas.Common.Models;      // For AuditableEntity, FormHistory, IWorkflowEntity, WorkFlowType, WorkFlowApprovalSequence, WorkFlowState
using ProcureToPay.Areas.Finance.Models;
using ProcureToPay.Areas.Insurance.Models;   // For InsuranceDocumentType, InsuranceCompany, InsuranceType  // For PurchaseRequisition (if it's in Inventory, otherwise put it in Procurement or Common if needed)
using ProcureToPay.Areas.Master.Models;      // For Branch, Department, Supplier, City, Region, Capacity, Charge, Make, Priority, Qualification, RateType, VehicleGroup, WorkOrderType, WHTaxExemption, WarningType, SubCategory, UoM, SKUCategory, SKUCategoryClient, SKUClient, SKUType, SKU, Shipper, Client, ClientInvoiceFormat, PaymentMode, IndustryVertical, InvoiceFormat, Company, ProductType, ProductNature, ServiceNature, Product, AssetDocument, DocumentType, Complainant, Consignee, LeaseType, Detention, Contractor, Relation, SeparationType, Driver, SupplierRate, SupplierRateDetail, SupplierType
using ProcureToPay.Areas.Procurement.Models; // For PurchaseRequest, PurchaseRequestItem, RequestApproval, ApprovalLevel, WorkFlowApprovalSequence, WorkFlowState, FormHistory, PurchaseOrder, PurchaseOrderItem, PurchaseRequestOrderMapping
using ProcureToPay.Areas.Receiving.Models;

// Add ALL necessary using directives for your models
using ProcureToPay.Areas.UserManagement.Models; // For User, Role, UserRole, Permission, RolePermission, Module, ModuleRoleHierarchy, Update
using ProcureToPay.Helpers;
using System.Security.Claims; // For ClaimTypes.NameIdentifier


namespace ProcureToPay.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, int, IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // User Management DbSets
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Module> Modules { get; set; }

        public DbSet<WorkFlowType> WorkFlowTypes { get; set; }
        public DbSet<ModuleRoleHierarchy> ModuleRoleHierarchies { get; set; }
        public DbSet<Update> Updates { get; set; }

        //Inventory Data DbSets
        public DbSet<PaymentRequest> PaymentRequests { get; set; }
        public DbSet<PaymentRequestDetail> PaymentRequestDetails { get; set; }
        public DbSet<PaymentRequestCostAllocation> PaymentRequestCostAllocations { get; set; }
        public DbSet<AttachmentType> AttachmentTypes { get; set; }
        public DbSet<PaymentRequestAttachment> PaymentRequestAttachments { get; set; }
        public DbSet<PurchaseRequestAttachment> PurchaseRequestAttachments { get; set; }

        // Master Data DbSets
        public DbSet<AccessorialCharge> AccessorialCharges { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<AssetTyre> AssetTyres { get; set; }
        public DbSet<AssetStatus> AssetStatuses { get; set; }
        public DbSet<AssetType> AssetTypes { get; set; }
        public DbSet<Base> Bases { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<Capacity> Capacities { get; set; }
        public DbSet<Charge> Charges { get; set; }
        public DbSet<Make> Makes { get; set; }
        public DbSet<Priority> Priorities { get; set; }
        public DbSet<Qualification> Qualifications { get; set; }
        public DbSet<RateType> RateTypes { get; set; }
        public DbSet<VehicleGroup> VehicleGroups { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SupplierRate> SupplierRates { get; set; }
        public DbSet<SupplierRateDetail> SupplierRateDetails { get; set; }
        public DbSet<SupplierType> SupplierTypes { get; set; }
        public DbSet<Trailer> Trailers { get; set; }
        public DbSet<WorkOrderType> WorkOrderTypes { get; set; }
        public DbSet<WHTaxExemption> WHTaxExemptions { get; set; }
        public DbSet<WarningType> WarningTypes { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<UoM> UoMs { get; set; }
        public DbSet<SKUCategory> SKUCategories { get; set; }
        public DbSet<SKUCategoryClient> SKUCategoryClients { get; set; }
        public DbSet<SKUClient> SKUClients { get; set; }
        public DbSet<SKUType> SKUTypes { get; set; }
        public DbSet<SubNature> SubNatures { get; set; }
        public DbSet<SKU> SKUs { get; set; }
        public DbSet<Shipper> Shippers { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<ClientInvoiceFormat> ClientInvoiceFormats { get; set; }
        public DbSet<PaymentMode> PaymentModes { get; set; }
        public DbSet<PaymentNature> PaymentNatures { get; set; }
        public DbSet<PaymentType> PaymentTypes { get; set; }
        public DbSet<IndustryVertical> IndustryVerticals { get; set; }
        public DbSet<InvoiceFormat> InvoiceFormats { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
        public DbSet<ProductNature> ProductNatures { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceNature> ServiceNatures { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<AssetDocument> AssetDocuments { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<Complainant> Complainants { get; set; }
        public DbSet<Consignee> Consignees { get; set; }
        public DbSet<LeaseType> LeaseTypes { get; set; }
        public DbSet<Detention> Detentions { get; set; }
        public DbSet<Contractor> Contractors { get; set; }
        public DbSet<Relation> Relations { get; set; }
        public DbSet<SeparationType> SeparationTypes { get; set; }
        public DbSet<Driver> Drivers { get; set; }

        // Insurance DbSets
        public DbSet<InsuranceDocumentType> InsuranceDocumentTypes { get; set; }
        public DbSet<InsuranceCompany> InsuranceCompanies { get; set; }
        public DbSet<InsuranceType> InsuranceTypes { get; set; }

        // Procurement DbSets
        public DbSet<PurchaseRequest> PurchaseRequests { get; set; }
        public DbSet<PurchaseRequestItem> PurchaseRequestDetails { get; set; }
        public DbSet<RequestApproval> RequestApprovals { get; set; }
        public DbSet<ApprovalLevel> ApprovalLevels { get; set; }
        public DbSet<WorkFlowApprovalSequence> WorkFlowApprovalSequences { get; set; }
        public DbSet<WorkFlowState> WorkFlowStates { get; set; }
        public DbSet<FormHistory> FormHistories { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<PurchaseRequestOrderMapping> PurchaseRequestOrderMappings { get; set; }
        public DbSet<PurchaseRequestsFleet> PurchaseRequestsFleets { get; set; }
        public DbSet<PurchaseRequestDetailFleet> PurchaseRequestDetailFleets { get; set; }
        public DbSet<GoodsReceiptNote> GoodsReceiptNotes { get; set; }
        public DbSet<GoodsReceiptNoteItem> GoodsReceiptNoteItems { get; set; }

        public DbSet<BidEvaluation> BidEvaluations { get; set; }

        public DbSet<Bid> Bids { get; set; }
        public DbSet<BidItem> BidItems { get; set; }

        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditFields()
        {
            var currentUserId = GetCurrentUserId();
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is AuditableEntity &&
                             (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var auditableEntity = (AuditableEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    auditableEntity.CreatedByUserId = currentUserId;
                    auditableEntity.CreatedOn = DateTimeHelper.GetPakistanStandardTime();
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditableEntity.UpdatedByUserId = currentUserId;
                    auditableEntity.UpdatedOn = DateTimeHelper.GetPakistanStandardTime();

                    // Prevent modification of Created fields
                    entry.Property(nameof(AuditableEntity.CreatedByUserId)).IsModified = false;
                    entry.Property(nameof(AuditableEntity.CreatedOn)).IsModified = false;
                }
            }
        }

        private int GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out var userId))
                {
                    return userId;
                }
            }
            return 0; // Return 0 or throw exception if user ID is mandatory
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure audit relationships for all entities inheriting from AuditableEntity
            ConfigureAuditRelationships(modelBuilder);

            modelBuilder.Entity<WorkFlowApprovalSequence>(entity =>
            {
                // Configure decimal properties for specific precision and scale
                entity.Property(e => e.MaxAmount)
                    .HasColumnType("decimal(18, 2)");
                entity.Property(e => e.MinAmount)
                    .HasColumnType("decimal(18, 2)");
            });

            // Configure Identity relationships
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });

                entity.HasOne(e => e.User)
                    .WithMany(e => e.UserRoles)
                    .HasForeignKey(e => e.UserId)
                    .IsRequired();

                entity.HasOne(e => e.Role)
                    .WithMany(e => e.UserRoles)
                    .HasForeignKey(e => e.RoleId)
                    .IsRequired();
            });

            // Configure RolePermission composite key
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.PermissionId });

                entity.HasOne(e => e.Role)
                    .WithMany(e => e.RolePermissions)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Permission)
                    .WithMany()
                    .HasForeignKey(e => e.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure User relationships
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasOne(e => e.Branch)
                    .WithMany(e => e.Users)
                    .HasForeignKey(e => e.BranchId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Department)
                    .WithMany(e => e.Users)
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Indexes for better performance
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.UserName).IsUnique();
                entity.HasIndex(e => new { e.BranchId, e.DepartmentId });
            });

            modelBuilder.Entity<Module>()
             .HasOne(m => m.WorkFlowType)
             .WithOne(wt => wt.Module)
             .HasForeignKey<WorkFlowType>(wt => wt.WorkFlowTypeId);

            // Configure PurchaseRequest relationships
            modelBuilder.Entity<PurchaseRequest>(entity =>
            {
                // User relationships
                entity.HasOne(e => e.RequestedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.RequestedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ApprovedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.ApprovedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Department and Branch relationships
                entity.HasOne(e => e.Department)
                    .WithMany()
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Branch)
                    .WithMany()
                    .HasForeignKey(e => e.BranchId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Indexes for performance
                entity.HasIndex(e => e.RequestNumber).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.RequestedByUserId);
                entity.HasIndex(e => e.DepartmentId);
                entity.HasIndex(e => e.RequestDate);

                // Decimal precision
                //    entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            });

            // Configure PurchaseRequestItem
            modelBuilder.Entity<PurchaseRequestItem>(entity =>
            {
                entity.HasOne(e => e.PurchaseRequest)
                    .WithMany(e => e.Items)
                    .HasForeignKey(e => e.PurchaseRequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Decimal precision
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
                entity.Property(e => e.Quantity).HasPrecision(18, 4); // Consistent with (18,4)

                // Computed column for TotalPrice - if it's truly a computed property in the DB, otherwise ignore.
                // entity.Ignore(e => e.TotalAmount);

                // Index for performance
                entity.HasIndex(e => e.PurchaseRequestId);
            });

            // Configure RequestApproval
            modelBuilder.Entity<RequestApproval>(entity =>
            {
                entity.HasOne(e => e.PurchaseRequest)
                    .WithMany(e => e.Approvals)
                    .HasForeignKey(e => e.PurchaseRequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ApprovalLevel)
                    .WithMany(e => e.RequestApprovals)
                    .HasForeignKey(e => e.ApprovalLevelId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Approver)
                    .WithMany()
                    .HasForeignKey(e => e.ApproverId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Unique constraint: One approval per request per level
                entity.HasIndex(e => new { e.PurchaseRequestId, e.ApprovalLevelId })
                    .IsUnique();

                // Performance indexes
                entity.HasIndex(e => e.ApproverId);
                entity.HasIndex(e => e.Status);
            });

            // Configure ApprovalLevel
            modelBuilder.Entity<ApprovalLevel>(entity =>
            {
                entity.Property(e => e.MinAmount).HasPrecision(18, 2);
                entity.Property(e => e.MaxAmount).HasPrecision(18, 2);

                entity.HasIndex(e => e.Order);
                entity.HasIndex(e => e.RequiredRole);
            });

            // Configure ModuleRoleHierarchy
            modelBuilder.Entity<ModuleRoleHierarchy>(entity =>
            {
                entity.HasOne(e => e.Role)
                    .WithMany(e => e.ModuleHierarchies)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.RoleId, e.ModuleName }).IsUnique();
            });

            // Configure Permission
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasOne(e => e.Module)
                    .WithMany()
                    .HasForeignKey(e => e.ModuleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.ModuleId, e.Name }).IsUnique();
            });

            // Configure AccessorialCharge
            modelBuilder.Entity<AccessorialCharge>(entity =>
            {
                entity.HasKey(e => e.ChargeId);
                entity.Property(e => e.ChargeId).ValueGeneratedOnAdd();
                entity.Property(e => e.ChargeName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ChargeCode).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.ChargeCode).IsUnique();
            });

            // Configure Activity
            modelBuilder.Entity<Activity>(entity =>
            {
                entity.HasKey(e => e.ActivityId);
                entity.Property(e => e.ActivityId).ValueGeneratedOnAdd();
                entity.Property(e => e.ActivityName).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.ActivityName).IsUnique();
                entity.Property(e => e.EstHrsReq).IsRequired();
            });

            // Configure Asset
            modelBuilder.Entity<Asset>(entity =>
            {
                entity.HasKey(e => e.AssetId);
                entity.HasIndex(e => e.AssetNo).IsUnique();
                entity.Property(e => e.AssetNo).IsRequired().HasMaxLength(50);
                entity.Property(e => e.StartKMs).HasPrecision(18, 2);
                entity.Property(e => e.KMs).HasPrecision(18, 2);
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasOne(a => a.AssetType)
                    .WithMany(at => at.Assets)
                    .HasForeignKey(a => a.AssetTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.CapacityName)
                    .WithMany()
                    .HasForeignKey(a => a.CapacityId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(a => a.MakeName)
                    .WithMany()
                    .HasForeignKey(a => a.MakeId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(a => a.LeaseTypeName)
                    .WithMany()
                    .HasForeignKey(a => a.LeaseTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.SupplierName)
                    .WithMany()
                    .HasForeignKey(a => a.SupplierId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(a => a.AssetStatus)
                    .WithMany()
                    .HasForeignKey(a => a.StatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.DriverName1)
                    .WithMany()
                    .HasForeignKey(a => a.DriverId1)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.DriverName2)
                    .WithMany()
                    .HasForeignKey(a => a.DriverId2)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.CityName)
                    .WithMany()
                    .HasForeignKey(a => a.CityId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.ClientName)
                    .WithMany()
                    .HasForeignKey(a => a.ClientId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(a => a.BaseName)
                    .WithMany()
                    .HasForeignKey(a => a.BaseId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(a => a.CompanyName)
                    .WithMany()
                    .HasForeignKey(a => a.CompanyId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(a => a.TrailerName)
                    .WithMany()
                    .HasForeignKey(a => a.TrailerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(a => a.AssetTyres)
                    .WithOne(at => at.Asset)
                    .HasForeignKey(at => at.AssetId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure AssetType
            modelBuilder.Entity<AssetType>(entity =>
            {
                entity.HasKey(e => e.TypeId);
                entity.Property(e => e.TypeId).ValueGeneratedOnAdd();
                entity.Property(e => e.TypeName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasIndex(e => e.TypeName).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configure AssetStatus
            modelBuilder.Entity<AssetStatus>(entity =>
            {
                entity.HasKey(e => e.StatusId);
                entity.Property(e => e.StatusId).ValueGeneratedOnAdd();
                entity.Property(e => e.StatusName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.Editable).HasDefaultValue(true);
                entity.HasIndex(e => e.StatusName).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configure AssetTyre
            modelBuilder.Entity<AssetTyre>(entity =>
            {
                entity.HasKey(e => e.DetailId);
                entity.Property(e => e.DetailId).ValueGeneratedOnAdd();
                entity.Property(e => e.SerialNo).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Make).IsRequired().HasMaxLength(100);
                entity.Property(e => e.StartKMs).HasPrecision(18, 2);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.StartKMs).HasDefaultValue(0);

                entity.HasOne(e => e.Asset)
                    .WithMany(e => e.AssetTyres)
                    .HasForeignKey(e => e.AssetId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.SerialNo).IsUnique();
                entity.HasIndex(e => e.AssetId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.AssetId, e.IsActive });
            });

            // Configure Base
            modelBuilder.Entity<Base>(entity =>
            {
                entity.HasKey(e => e.BaseId);
                entity.Property(e => e.BaseId).ValueGeneratedOnAdd();
                entity.Property(e => e.BaseName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasIndex(e => e.BaseName).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configure Region
            modelBuilder.Entity<Region>(entity =>
            {
                entity.HasKey(e => e.RegionId);
                entity.Property(e => e.RegionId).ValueGeneratedOnAdd();
                entity.Property(e => e.RegionName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TaxRate).HasDefaultValue(0.0);
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasMany(e => e.Cities)
                    .WithOne(e => e.Region)
                    .HasForeignKey(e => e.RegionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.RegionName).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configure City
            modelBuilder.Entity<City>(entity =>
            {
                entity.HasKey(e => e.CityId);
                entity.Property(e => e.CityId).ValueGeneratedOnAdd();
                entity.Property(e => e.CityCode).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CityName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasOne(e => e.Region)
                    .WithMany(e => e.Cities)
                    .HasForeignKey(e => e.RegionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.CityCode).IsUnique();
                entity.HasIndex(e => e.CityName).IsUnique();
                entity.HasIndex(e => e.RegionId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.RegionId, e.IsActive });
            });

            // Priority configuration
            modelBuilder.Entity<Priority>(entity =>
            {
                entity.HasKey(e => e.PriorityId);
                entity.Property(e => e.PriorityName)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.HasIndex(e => e.PriorityName)
                    .IsUnique();
            });

            // Qualification configuration
            modelBuilder.Entity<Qualification>(entity =>
            {
                entity.HasKey(e => e.QualificationId);
                entity.Property(e => e.QualificationName)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.HasIndex(e => e.QualificationName)
                    .IsUnique();
            });

            // RateType configuration
            modelBuilder.Entity<RateType>(entity =>
            {
                entity.HasKey(e => e.RateTypeId);
                entity.Property(e => e.RateTypeId).ValueGeneratedOnAdd();
                entity.Property(e => e.RateTypeName)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.HasIndex(e => e.RateTypeName)
                    .IsUnique();
            });

            modelBuilder.Entity<SupplierType>(entity =>
            {
                entity.HasKey(e => e.TypeId);
                entity.Property(e => e.TypeId).ValueGeneratedOnAdd();
                entity.Property(e => e.SupplierTypeName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasIndex(e => e.SupplierTypeName).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configure Supplier (Your existing Supplier model in Master.Models)
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasKey(e => e.SupplierId);
                entity.Property(e => e.SupplierId).ValueGeneratedOnAdd();

                entity.Property(e => e.SupplierName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.PhoneNo).HasMaxLength(20);
                entity.Property(e => e.FaxNo).HasMaxLength(20);
                entity.Property(e => e.ContactName).HasMaxLength(100);
                entity.Property(e => e.MobileNo).HasMaxLength(20);
                entity.Property(e => e.NTN).HasMaxLength(50);
                entity.Property(e => e.URL).HasMaxLength(200);
                entity.Property(e => e.ControlSupplierId).HasMaxLength(50);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.SCRate).HasDefaultValue(0.0);

                // Configure relationships for Supplier
                entity.HasOne(e => e.SupplierType)
                    .WithMany()
                    .HasForeignKey(e => e.SupplierTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.City)
                    .WithMany()
                    .HasForeignKey(e => e.CityId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Indexes for performance
                entity.HasIndex(e => e.SupplierName);
                entity.HasIndex(e => e.SupplierTypeId);
                entity.HasIndex(e => e.CityId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.SupplierTypeId, e.IsActive });
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.NTN);
            });

            // Configure SupplierRate
            modelBuilder.Entity<SupplierRate>(entity =>
            {
                entity.HasKey(e => e.SupplierRateId);
                entity.Property(e => e.SupplierRateId).ValueGeneratedOnAdd();
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasOne(e => e.Supplier)
                    .WithMany(s => s.SupplierRates) // Corrected: Supplier has ICollection<SupplierRate>
                    .HasForeignKey(e => e.SupplierId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Details)
                    .WithOne(e => e.SupplierRate)
                    .HasForeignKey(e => e.SupplierRateId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.SupplierId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.SupplierId, e.IsActive });
            });

            // Configure SupplierRateDetail
            modelBuilder.Entity<SupplierRateDetail>(entity =>
            {
                entity.HasKey(e => e.DetailId);
                entity.Property(e => e.DetailId).ValueGeneratedOnAdd();
                entity.Property(e => e.FromDate).IsRequired().HasColumnType("datetime");
                entity.Property(e => e.ToDate).IsRequired().HasColumnType("datetime");
                entity.Property(e => e.FuelRate).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasOne(e => e.SupplierRate)
                    .WithMany(e => e.Details)
                    .HasForeignKey(e => e.SupplierRateId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.SupplierRateId);
                entity.HasIndex(e => e.FromDate);
                entity.HasIndex(e => e.ToDate);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.SupplierRateId, e.IsActive });
                entity.HasIndex(e => new { e.FromDate, e.ToDate });
                entity.HasIndex(e => new { e.SupplierRateId, e.FromDate, e.ToDate })
                    .IsUnique()
                    .HasDatabaseName("IX_SupplierRateDetail_UniqueRate");
            });

            // Configure Trailer
            modelBuilder.Entity<Trailer>(entity =>
            {
                entity.HasKey(e => e.TrailerId);
                entity.Property(e => e.TrailerId).ValueGeneratedOnAdd();
                entity.Property(e => e.TrailerName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasIndex(e => e.TrailerName).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configure Capacity
            modelBuilder.Entity<Capacity>(entity =>
            {
                entity.HasKey(e => e.CapacityId);
                entity.Property(e => e.CapacityId).ValueGeneratedOnAdd();
                entity.Property(e => e.CapacityName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasIndex(e => e.CapacityName).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configure Charge
            modelBuilder.Entity<Charge>(entity =>
            {
                entity.HasKey(e => e.ChargeId);
                entity.Property(e => e.ChargeId).ValueGeneratedOnAdd();
                entity.Property(e => e.ChargeName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasIndex(e => e.ChargeName).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configure VehicleGroup
            modelBuilder.Entity<VehicleGroup>(entity =>
            {
                entity.HasKey(e => e.GroupId);
                entity.Property(e => e.GroupId).ValueGeneratedOnAdd();
                entity.Property(e => e.GroupName).IsRequired().HasMaxLength(20);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasIndex(e => e.GroupName).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configure WorkOrderType
            modelBuilder.Entity<WorkOrderType>(entity =>
            {
                entity.HasKey(e => e.WorkOrderTypeId);
                entity.Property(e => e.WorkOrderTypeId).ValueGeneratedOnAdd();
                entity.Property(e => e.WorkOrderTypeName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.WorkOrderTypeCode).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
                entity.HasIndex(e => e.WorkOrderTypeName).IsUnique();
                entity.HasIndex(e => e.WorkOrderTypeCode).IsUnique();
                entity.HasIndex(e => e.WorkFlowId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.DisplayOrder);
            });

            // Configure WHTaxExemption
            modelBuilder.Entity<WHTaxExemption>(entity =>
            {
                entity.HasKey(e => e.ExemptionId);
                entity.Property(e => e.ExemptionId).ValueGeneratedOnAdd();
                entity.Property(e => e.DateFrom).IsRequired().HasColumnType("datetime");
                entity.Property(e => e.DateTo).IsRequired().HasColumnType("datetime");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.DateFrom);
                entity.HasIndex(e => e.DateTo);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.CompanyId, e.DateFrom, e.DateTo });
            });

            // Configure WarningType
            modelBuilder.Entity<WarningType>(entity =>
            {
                entity.HasKey(e => e.TypeId);
                entity.Property(e => e.TypeId).ValueGeneratedOnAdd();
                entity.Property(e => e.TypeName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasIndex(e => e.TypeName).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configure SeparationType
            modelBuilder.Entity<SeparationType>(entity =>
            {
                entity.HasKey(e => e.TypeId);
                entity.Property(e => e.TypeId).ValueGeneratedOnAdd();
                entity.Property(e => e.TypeName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasIndex(e => e.TypeName).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configure SubCategory
            modelBuilder.Entity<SubCategory>(entity =>
            {
                entity.HasKey(e => e.SubCategoryId);
                entity.Property(e => e.SubCategoryId).ValueGeneratedOnAdd();
                entity.Property(e => e.SubCategoryName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasIndex(e => e.SubCategoryName).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configure UoM
            modelBuilder.Entity<UoM>(entity =>
            {
                entity.HasKey(e => e.UoMId);
                entity.Property(e => e.UoMId).ValueGeneratedOnAdd();
                entity.Property(e => e.UoMName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasIndex(e => e.UoMName).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configure SKUCategory
            modelBuilder.Entity<SKUCategory>(entity =>
            {
                entity.HasKey(e => e.CategoryId);
                entity.Property(e => e.CategoryId).ValueGeneratedOnAdd();
                entity.Property(e => e.CategoryName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasMany(e => e.CategoryClients)
                    .WithOne(e => e.Category)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.CategoryName).IsUnique();
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.CompanyId, e.IsActive });
            });

            // Configure SKUCategoryClient
            modelBuilder.Entity<SKUCategoryClient>(entity =>
            {
                entity.HasKey(e => e.DetailId);
                entity.Property(e => e.DetailId).ValueGeneratedOnAdd();

                entity.HasOne(e => e.Category)
                    .WithMany(e => e.CategoryClients)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.ClientId);
                entity.HasIndex(e => new { e.CategoryId, e.ClientId }).IsUnique();
            });

            // Configure SKUClient
            modelBuilder.Entity<SKUClient>(entity =>
            {
                entity.HasKey(e => e.DetailId);
                entity.Property(e => e.DetailId).ValueGeneratedOnAdd();

                entity.HasOne(e => e.SKU)
                    .WithMany(e => e.SKUClients)
                    .HasForeignKey(e => e.SKUId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Client)
                    .WithMany()
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.SKUId);
                entity.HasIndex(e => e.ClientId);
                entity.HasIndex(e => new { e.SKUId, e.ClientId }).IsUnique();
            });

            // Configure SKUType
            modelBuilder.Entity<SKUType>(entity =>
            {
                entity.HasKey(e => e.TypeId);
                entity.Property(e => e.TypeId).ValueGeneratedOnAdd();
                entity.Property(e => e.TypeName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasMany(e => e.SKUs)
                    .WithOne(e => e.SKUType)
                    .HasForeignKey(e => e.SKUTypeId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.TypeName).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configure SKU
            modelBuilder.Entity<SKU>(entity =>
            {
                entity.HasKey(e => e.SKUId);
                entity.Property(e => e.SKUId).ValueGeneratedOnAdd();
                entity.Property(e => e.SKUName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasOne(e => e.SKUType)
                    .WithMany(e => e.SKUs)
                    .HasForeignKey(e => e.SKUTypeId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(e => e.SKUClients)
                    .WithOne(e => e.SKU)
                    .HasForeignKey(e => e.SKUId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.SKUName).IsUnique();
                entity.HasIndex(e => e.SKUTypeId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.SKUTypeId, e.IsActive });
            });

            // Configure Shipper
            modelBuilder.Entity<Shipper>(entity =>
            {
                entity.HasKey(e => e.ShipperId);
                entity.Property(e => e.ShipperId).ValueGeneratedOnAdd();
                entity.Property(e => e.ShipperName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(255);
                entity.Property(e => e.ContactNo).HasMaxLength(50);
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasOne(e => e.City)
                    .WithMany()
                    .HasForeignKey(e => e.CityId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Client)
                    .WithMany()
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.ShipperName);
                entity.HasIndex(e => e.CityId);
                entity.HasIndex(e => e.ClientId);
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.CompanyId, e.IsActive });
            });

            // Configure Client
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.ClientId);
                entity.Property(e => e.ClientId).ValueGeneratedOnAdd();
                entity.Property(e => e.ClientName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ShortName).HasMaxLength(50);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.ContactNo).HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.URL).HasMaxLength(200);
                entity.Property(e => e.ContactPerson).HasMaxLength(100);
                entity.Property(e => e.CWClientId).HasMaxLength(50);
                entity.Property(e => e.NTN).HasMaxLength(50);
                entity.Property(e => e.STRN).HasMaxLength(50);
                entity.Property(e => e.CreditLimit).HasPrecision(18, 2);
                entity.Property(e => e.TaxRate).HasPrecision(5, 2);
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasOne(e => e.City)
                    .WithMany()
                    .HasForeignKey(e => e.CityId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.IndustryVertical)
                    .WithMany()
                    .HasForeignKey(e => e.IndustryVerticalId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.PaymentMode)
                    .WithMany()
                    .HasForeignKey(e => e.PaymentModeId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.RateType)
                    .WithMany()
                    .HasForeignKey(e => e.RateTypeId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Company)
                    .WithMany()
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.ClientInvoiceFormats)
                    .WithOne(e => e.Client)
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.ClientName);
                entity.HasIndex(e => e.ShortName);
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.NTN);
                entity.HasIndex(e => e.CityId);
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.CompanyId, e.IsActive });
            });

            // Configure ClientInvoiceFormat
            modelBuilder.Entity<ClientInvoiceFormat>(entity =>
            {
                entity.HasKey(e => e.DetailId);
                entity.Property(e => e.DetailId).ValueGeneratedOnAdd();

                entity.HasOne(e => e.Client)
                    .WithMany(e => e.ClientInvoiceFormats)
                    .HasForeignKey(e => e.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.InvoiceFormat)
                    .WithMany()
                    .HasForeignKey(e => e.FormatId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.ClientId);
                entity.HasIndex(e => e.FormatId);
                entity.HasIndex(e => new { e.ClientId, e.FormatId }).IsUnique();
            });

            // Configure PaymentMode
            modelBuilder.Entity<PaymentMode>(entity =>
            {
                entity.HasKey(e => e.PaymentModeId);
                entity.Property(e => e.PaymentModeId).ValueGeneratedOnAdd();
                entity.Property(e => e.PaymentModeName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasIndex(e => e.PaymentModeName).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configure IndustryVertical
            modelBuilder.Entity<IndustryVertical>(entity =>
            {
                entity.HasKey(e => e.IndustryVerticalId);
                entity.Property(e => e.IndustryVerticalId).ValueGeneratedOnAdd();
                entity.Property(e => e.IndustryVerticalName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasIndex(e => e.IndustryVerticalName).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configure InvoiceFormat
            modelBuilder.Entity<InvoiceFormat>(entity =>
            {
                entity.HasKey(e => e.FormatId);
                entity.Property(e => e.FormatId).ValueGeneratedOnAdd();
                entity.Property(e => e.FormatName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasIndex(e => e.FormatName).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // Configure Company
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(e => e.CompanyId);
                entity.Property(e => e.CompanyId).ValueGeneratedOnAdd();
                entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CompanyAddress).HasMaxLength(500);
                entity.Property(e => e.NTN).HasMaxLength(50);
                entity.Property(e => e.PeriodName).HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.Property(e => e.EnableGL).HasDefaultValue(false);
                entity.Property(e => e.EnablePartialDelivery).HasDefaultValue(false);
                entity.Property(e => e.RouteByConsignee).HasDefaultValue(false);
                entity.Property(e => e.SeparateFixedInvoice).HasDefaultValue(false);
                entity.Property(e => e.IsMandatoryDriver2).HasDefaultValue(false);
                entity.Property(e => e.AllowTrailer).HasDefaultValue(false);

                entity.HasIndex(e => e.CompanyName).IsUnique();
                entity.HasIndex(e => e.NTN);
                entity.HasIndex(e => e.IsActive);
            });

            modelBuilder.Entity<BidEvaluation>(entity =>
            {
                entity.ToTable("BidEvaluations", schema: "Procurement");
                entity.HasKey(e => e.BidNo);
                entity.HasIndex(e => e.BidEvaluationNumber).IsUnique();

                entity.HasOne(e => e.PurchaseRequest)
                    .WithMany(pr => pr.BidEvaluations)
                    .HasForeignKey(e => e.PRNo)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.SelectedBid)
                    .WithMany()
                    .HasForeignKey(e => e.SelectedBidId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Bid Configuration
            modelBuilder.Entity<Bid>(entity =>
            {
                entity.ToTable("Bids", schema: "Procurement");
                entity.HasKey(b => b.BidId);

                entity.HasOne(b => b.BidEvaluation)
                    .WithMany(be => be.Bids)
                    .HasForeignKey(b => b.BidNo)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(b => b.Supplier)
                    .WithMany()
                    .HasForeignKey(b => b.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // BidItem Configuration
            modelBuilder.Entity<BidItem>(entity =>
            {
                entity.ToTable("BidItems", schema: "Procurement");
                entity.HasKey(bi => bi.BidItemId);

                entity.HasOne(bi => bi.Bid)
                    .WithMany(b => b.BidItems)
                    .HasForeignKey(bi => bi.BidId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(bi => bi.PurchaseRequestItem)
                    .WithMany()
                    .HasForeignKey(bi => bi.PurchaseRequestItemId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            // Add configurations for Procurement DbSets (PurchaseOrder, PurchaseOrderItem)
            modelBuilder.Entity<PurchaseOrder>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.PONumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.PONumber).IsUnique();

                entity.Property(e => e.PODate).IsRequired().HasColumnType("datetime2");
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
                entity.Property(e => e.GrandTotal).HasPrecision(18, 2);

                // CORRECTED: Use VendorId as the foreign key to Supplier
                entity.HasOne(po => po.Supplier)
                    .WithMany(s => s.PurchaseOrders) // Ensure Supplier model has ICollection<PurchaseOrder>
                    .HasForeignKey(po => po.VendorId) // CORRECTED: from SupplierId to VendorId
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(po => po.Items)
                    .WithOne(poi => poi.PurchaseOrder)
                    .HasForeignKey(poi => poi.PurchaseOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes for performance
                entity.HasIndex(e => e.PODate);
                entity.HasIndex(e => e.VendorId); // CORRECTED: from SupplierId to VendorId
                entity.HasIndex(e => e.Status);
            });

            modelBuilder.Entity<PurchaseOrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.ItemName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Unit).HasMaxLength(50);
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);

                entity.Property(e => e.Quantity).HasPrecision(18, 4);
                entity.Property(e => e.GSTRate).HasPrecision(5, 2);
                entity.Property(e => e.GSTAmount).HasPrecision(18, 2);
                entity.Property(e => e.DiscRate).HasPrecision(5, 2);
                entity.Property(e => e.DiscAmount).HasPrecision(18, 2);
                entity.Property(e => e.TaxRate).HasPrecision(5, 2);
                entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
                entity.Property(e => e.TotalPrice).HasPrecision(18, 2);

                // CORRECTED: Use SourcePurchaseRequestItem and SourcePurchaseRequestItemId
                entity.HasOne(poi => poi.SourcePurchaseRequestItem) // CORRECTED: from PurchaseRequestItem to SourcePurchaseRequestItem
                    .WithMany() // A PR item can be linked to many PO items (for partial ordering)
                    .HasForeignKey(poi => poi.SourcePurchaseRequestItemId) // CORRECTED: from PurchaseRequestItemId to SourcePurchaseRequestItemId
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes for performance
                entity.HasIndex(e => e.PurchaseOrderId);
                entity.HasIndex(e => e.SourcePurchaseRequestItemId); // CORRECTED: from PurchaseRequestItemId to SourcePurchaseRequestItemId
                entity.HasIndex(e => e.Status);
            });

            // Configure PurchaseRequestOrderMapping
            modelBuilder.Entity<PurchaseRequestOrderMapping>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.MappedAmount).HasPrecision(18, 2);
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasOne(e => e.PurchaseRequest)
                    .WithMany(pr => pr.PurchaseRequestMappings)
                    .HasForeignKey(e => e.PurchaseRequestId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PurchaseOrder)
                    .WithMany(po => po.PurchaseRequestMappings)
                    .HasForeignKey(e => e.PurchaseOrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.PurchaseRequestId);
                entity.HasIndex(e => e.PurchaseOrderId);
                entity.HasIndex(e => new { e.PurchaseRequestId, e.PurchaseOrderId }).IsUnique();
            });

            modelBuilder.Entity<GoodsReceiptNote>()
                .HasMany(grn => grn.Items)
                .WithOne(item => item.GoodsReceiptNote)
                .HasForeignKey(item => item.GoodsReceiptNoteId)
                .OnDelete(DeleteBehavior.Cascade); // Adjust delete behavior as needed

            modelBuilder.Entity<GoodsReceiptNoteItem>()
                .HasOne(grni => grni.PurchaseOrderItem)
                .WithMany() // Or specify a navigation property on PurchaseOrderItem if you add one
                .HasForeignKey(grni => grni.PurchaseOrderItemId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting PO item if it has GRN items
        }

        private void ConfigureAuditRelationships(ModelBuilder modelBuilder)
        {
            var auditableEntities = modelBuilder.Model.GetEntityTypes()
                .Where(e => typeof(AuditableEntity).IsAssignableFrom(e.ClrType));

            foreach (var entityType in auditableEntities)
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasOne(typeof(User), "CreatedByUser")
                    .WithMany()
                    .HasForeignKey("CreatedByUserId")
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity(entityType.ClrType)
                    .HasOne(typeof(User), "UpdatedByUser")
                    .WithMany()
                    .HasForeignKey("UpdatedByUserId")
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex("CreatedByUserId");

                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex("CreatedOn");
            }

            modelBuilder.Entity<FormHistory>(entity =>
            {
                entity.Property(fh => fh.FormId)
                    .IsRequired();
                entity.Ignore(fh => fh.WorkFlowType);
                entity.HasOne(fh => fh.ActionByUser)
                    .WithMany()
                    .HasForeignKey(fh => fh.ActionByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}