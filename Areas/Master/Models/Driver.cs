using ProcureToPay.Areas.Common.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProcureToPay.Areas.Master.Models
{

    [Table("Drivers")]
    public class Driver : AuditableEntity
    {
        [Key] // Marks this property as the primary key
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
        public string? EmployeeNo { get; set; } // Nullable

        [Display(Name = "Joining Date")]
        [DataType(DataType.DateTime)]
        public DateTime JoiningDate { get; set; }

        [Display(Name = "Contractor")]
        public short? ContractorId { get; set; } // Nullable

        [Display(Name = "Monthly Salary")]
        [Column(TypeName = "numeric(9, 2)")]
        public decimal MonthlySalary { get; set; }

        [Display(Name = "Address")]
        [StringLength(200)]
        public string Address { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Display(Name = "Experience (Years)")]
        [Column(TypeName = "numeric(3, 1)")]
        public decimal Experience { get; set; }

        [Display(Name = "Cell Number")]
        [StringLength(15)]
        [Phone]
        public string CellNo { get; set; }

        [Display(Name = "License Number")]
        [StringLength(20)]
        public string LicenseNo { get; set; }

        [Display(Name = "License Expiry Date")]
        [DataType(DataType.DateTime)]
        public DateTime LicenseExpiry { get; set; }

        [Display(Name = "CNIC")]
        [StringLength(15)]
        public string CNIC { get; set; }

        
        [DataType(DataType.DateTime)]
        public DateTime CNICExpiry { get; set; }

        [Display(Name = "Qualification")]
        public short? QualificationId { get; set; } // Nullable

        [Display(Name = "Next of Kin Name")]
        [StringLength(50)]
        public string NoKName { get; set; }

        [Display(Name = "Next of Kin Relation")]
        public short? NoKRelationId { get; set; } // Nullable

        [Display(Name = "Previous Employer")]
        [StringLength(100)]
        public string? PreviousEmployer { get; set; } // Nullable

        [Display(Name = "Separation Type")]
        public short? SeparationTypeId { get; set; } // Nullable

        [Display(Name = "Separation Date")]
        [DataType(DataType.DateTime)]
        public DateTime? SeparationDate { get; set; } // Nullable

        [Display(Name = "Separation Reason")]
        [StringLength(500)]
        public string? SeparationReason { get; set; } // Nullable

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }

        [ForeignKey("ContractorId")]
        public virtual Contractor Contractor { get; set; }

        [ForeignKey("QualificationId")]
        public virtual Qualification Qualification { get; set; }

        [ForeignKey("SeparationTypeId")]
        public virtual SeparationType SeparationType { get; set; }
    }
}
