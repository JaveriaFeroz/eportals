using Microsoft.AspNetCore.Mvc.Rendering;
using ProcureToPay.Areas.Common.Enums;
using ProcureToPay.Areas.Master.Models;
using ProcureToPay.Areas.UserManagement.Models;
using PaymentNature = ProcureToPay.Areas.Master.Models.PaymentNature;

namespace ProcureToPay.Areas.UserManagement.ViewModels
{
    public class ModuleHierarchyViewModel
    {
        public string ModuleName { get; set; }
        public string ModuleDisplayName { get; set; }
        public IEnumerable<ModuleRoleHierarchy> RoleHierarchies { get; set; }
        public IEnumerable<Role> AvailableRoles { get; set; }
    }

    public class PaymentRequestHierarchyViewModel
    {
        public string ModuleName { get; set; }
        public string ModuleDisplayName { get; set; }
        public IEnumerable<ModuleRoleHierarchy> RoleHierarchies { get; set; }
        public IEnumerable<Role> AvailableRoles { get; set; }

        // Corrected properties to use SelectListItem
        public IEnumerable<SelectListItem> PaymentNatures { get; set; }
        public IEnumerable<SelectListItem> PaymentSubNatures { get; set; }

        public int PaymentNatureId { get; set; }
        public int PaymentSubNatureId { get; set; }
    }

    public class SelectPaymentHierarchyViewModel
    {
        public string ModuleName { get; set; }
        public string ModuleDisplayName { get; set; }
        public List<PaymentNature> PaymentNatures { get; set; }
        public List<SubNature> PaymentSubNatures { get; set; }
        public Dictionary<(short? paymentNatureId, short? paymentSubNatureId), bool> ExistingHierarchies { get; set; }
    }
}
