using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.DiscountRules.ItemsBelowPrice.Models
{
    public record RequirementModel
    {
        public int DiscountId { get; set; }

        public int RequirementId { get; set; }
        [NopResourceDisplayName("Plugins.DiscountRules.ItemsBelowPrice.Fields.Amount")]
   
        public decimal Amount { get; set; }
    }
}