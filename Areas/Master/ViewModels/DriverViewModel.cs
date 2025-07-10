using Microsoft.AspNetCore.Mvc.Rendering;
using ProcureToPay.Areas.Master.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Areas.Master.Models
{
    public class DriverViewModel
    {
        public short DriverId { get; set; }

        [Required]
        [Display(Name = "Company")]
        public short CompanyId { get; set; }

        [Required(ErrorMessage = "Driver Name is required")]
        [Display(Name = "Driver Name")]
        [StringLength(50)]
        public string DriverName { get; set; }

        [Display(Name = "Father Name")]
        [StringLength(50)]
        public string FatherName { get; set; }

        [Required]
        [Display(Name = "Branch")]
        public short BranchId { get; set; }

        [Display(Name = "Designation")]
        [StringLength(50)]
        public string Designation { get; set; }

        [Display(Name = "Employee Number")]
        [StringLength(10)]
        public string? EmployeeNo { get; set; }

        [Required(ErrorMessage = "Joining Date is required")]
        [Display(Name = "Joining Date")]
        [DataType(DataType.Date)]
        public DateTime JoiningDate { get; set; } = DateTime.Today;

        [Display(Name = "Contractor")]
        public short? ContractorId { get; set; }

        [Required(ErrorMessage = "Monthly Salary is required")]
        [Display(Name = "Monthly Salary")]
        [Range(0, 9999999.99, ErrorMessage = "Salary must be a positive number.")]
        public decimal MonthlySalary { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [Display(Name = "Address")]
        [StringLength(200)]
        public string Address { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; } = DateTime.Today.AddYears(-20);

        [Required(ErrorMessage = "Experience is required")]
        [Display(Name = "Experience (Years)")]
        [Range(0, 99.9, ErrorMessage = "Experience must be a positive number.")]
        public decimal Experience { get; set; }

        [Required(ErrorMessage = "Cell Number is required")]
        [Display(Name = "Cell Number")]
        [StringLength(15)]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string CellNo { get; set; }

        [Required(ErrorMessage = "License Number is required")]
        [Display(Name = "License Number")]
        [StringLength(20)]
        public string LicenseNo { get; set; }

        [Required(ErrorMessage = "License Expiry Date is required")]
        [Display(Name = "License Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime LicenseExpiry { get; set; } = DateTime.Today.AddYears(1);

        [Required(ErrorMessage = "CNIC is required")]
        [Display(Name = "CNIC")]
        [StringLength(15, MinimumLength = 13, ErrorMessage = "CNIC must be 13 digits without dashes.")]
        [RegularExpression("^[0-9]{13}$", ErrorMessage = "CNIC must be 13 digits without dashes.")]
        public string CNIC { get; set; }

        [Required(ErrorMessage = "CNIC Expiry Date is required")]
        [Display(Name = "CNIC Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime CNICExpiry { get; set; } = DateTime.Today.AddYears(1);

        [Display(Name = "Qualification")]
        public short? QualificationId { get; set; }

        [Required(ErrorMessage = "Next of Kin Name is required")]
        [Display(Name = "Next of Kin Name")]
        [StringLength(50)]
        public string NoKName { get; set; }

        [Display(Name = "Next of Kin Relation")]
        public short? NoKRelationId { get; set; }

        [Display(Name = "Previous Employer")]
        [StringLength(100)]
        public string? PreviousEmployer { get; set; }

        [Display(Name = "Separation Type")]
        public short? SeparationTypeId { get; set; }

        [Display(Name = "Separation Date")]
        [DataType(DataType.Date)]
        public DateTime? SeparationDate { get; set; }

        [Display(Name = "Separation Reason")]
        [StringLength(500)]
        public string? SeparationReason { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Read-only audit fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }

        // Display names for foreign keys
        public string? CompanyName { get; set; }
        public string? BranchName { get; set; }
        public string? ContractorName { get; set; }
        public string? QualificationName { get; set; }
        public string? NoKRelationName { get; set; }
        public string? SeparationTypeName { get; set; }
    }

    public class DriverListViewModel
    {
        public short DriverId { get; set; }
        public string CompanyName { get; set; }
        public string DriverName { get; set; }

        public string FatherName { get; set; }
        public string BranchName { get; set; }
        public string Designation { get; set; }
        public string EmployeeNo { get; set; }

        public string JoiningDate { get; set; }

        public string ContractorName { get; set; }

        public string MonthlySalary { get; set; }
        public string Address { get; set; }
        public string BirthDate { get; set; }
        public string Experience { get; set; }
        public string CellNo { get; set; }
        public string LicenseNo { get; set; }
        public string LicenseExpiry { get; set; }
        public string CNIC { get; set; }
        public string CNICExpiry { get; set; }

        public string QualificationName { get; set; }
        public string NoKName { get; set; }

        public string PreviousEmployer { get; set; }
        public string SeparationTypeName { get; set; }
        public string SeparationDate { get; set; }
        public string SeparationReason { get; set; }

        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    /// <summary>
    /// ViewModel for the main Driver index page, including search and filter options.
    /// </summary>
    public class DriverIndexViewModel
    {
        public List<DriverListViewModel> Drivers { get; set; } = new();
        public bool ShowInactiveOnly { get; set; } = false;
        public string SearchTerm { get; set; }
    }


    public class DriverCreateEditViewModel
    {
        public DriverViewModel Driver { get; set; } = new DriverViewModel();
        public List<SelectListItem> Companies { get; set; } = new();

        public List<SelectListItem> Branches { get; set; } = new();

        // This will hold the list for the city dropdown
        public List<SelectListItem> Contractors { get; set; } = new();

        public List<SelectListItem> Qualifications { get; set; } = new();

        public List<SelectListItem> NOKRelations { get; set; } = new();

        public List<SelectListItem> SeparationTypes { get; set; } = new();

    }

    public class DriverLookupsViewModel
    {
        public List<CompanyLookupViewModel> Companies { get; set; } = new List<CompanyLookupViewModel>();
        public List<BranchLookupViewModel> Branches { get; set; } = new List<BranchLookupViewModel>();
        public List<ContractorLookupViewModel> Contractors { get; set; } = new List<ContractorLookupViewModel>();
        public List<QualificationLookupViewModel> Qualifications { get; set; } = new List<QualificationLookupViewModel>();

        public List<NOKRelationLookupViewModel> NOKRelations { get; set; } = new List<NOKRelationLookupViewModel>();
        public List<SeparationTypeLookupViewModel> SeparationTypes { get; set; } = new List<SeparationTypeLookupViewModel>();
    }

    public class DriverLookupViewModel
    {
        public short DriverId { get; set; }
        public string DriverName { get; set; }
    }
    public class BranchLookupViewModel
    {
        public short BranchId { get; set; }
        public string BranchName { get; set; }
    }
    public class NOKRelationLookupViewModel
    {
        public short RelationId { get; set; }
        public string RelationName { get; set; }
    }

}