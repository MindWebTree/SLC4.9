using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace MWT.Plugin.Misc.MwtStorefront.Models.CustomOrder
{
    public partial record CustomOrderLoginInfo: BaseNopModel
    {
        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Account.Fields.Email")]
        public string EmailAddress { get; set; }

        [NopResourceDisplayName("Account.Fields.ZipPostalCode")]
        public string ZipCode { get; set; }

        public int OrderId { get; set; }

        public int ParentOrderId { get; set; }
    }
}
