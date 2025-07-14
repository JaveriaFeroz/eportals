using System.ComponentModel.DataAnnotations;

namespace ProcureToPay.Enums
{
    public enum PurchaseNatureType
    {
        [Display(Name = "Select Type")]
        None = 0,
        [Display(Name = "Opex")]
        Opex = 1,
        [Display(Name = "Capex")]
        Capex = 2
    }

    public enum PurchaseItemType
    {
        [Display(Name = "Select Item Type")]
        None = 0,
        [Display(Name = "Goods")]
        Goods = 1,
        [Display(Name = "Service")]
        Service = 2
    }
}