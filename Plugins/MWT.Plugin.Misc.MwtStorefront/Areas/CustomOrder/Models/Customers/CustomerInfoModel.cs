using Nop.Core;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Customers
{
    public class CustomerInfoModel:BaseEntity
    {
        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Account.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Account.Fields.FirstName")]
        public string FirstName { get; set; }

        [NopResourceDisplayName("Account.Fields.LastName")]
        public string LastName { get; set; }

        [NopResourceDisplayName("Account.Fields.ZipPostalCode")]
        public string ZipPostalCode { get; set; }

        [DataType(DataType.PhoneNumber)]
        [NopResourceDisplayName("Account.Fields.Phone")]
        public string Phone { get; set; }
    }
}
