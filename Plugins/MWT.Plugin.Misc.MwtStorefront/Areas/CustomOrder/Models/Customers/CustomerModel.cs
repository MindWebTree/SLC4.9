using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Customers
{
    public partial record CustomerModel : BaseNopEntityModel
    {
        #region Ctor

        public CustomerModel()
        {
            DefaultShippingAddress = new AddressModel();
            DefaultBillingAddress = new AddressModel();
        }

        #endregion

        #region Properties


        [NopResourceDisplayName("CustomOrder.Customers.Customers.Fields.Username")]
        public string Username { get; set; }

        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("CustomOrder.Customers.Customers.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("CustomOrder.Customers.Customers.Fields.FirstName")]
        public string FirstName { get; set; }

        [NopResourceDisplayName("CustomOrder.Customers.Customers.Fields.LastName")]
        public string LastName { get; set; }

        [NopResourceDisplayName("CustomOrder.Customers.Customers.Fields.FullName")]
        public string FullName { get; set; }

        [NopResourceDisplayName("CustomOrder.Customers.Customers.Fields.Location")]
        public string Location { get; set; }

        [NopResourceDisplayName("CustomOrder.Customers.Customers.Fields.NoOforders")]
        public int NoOforders { get; set; }

        [NopResourceDisplayName("CustomOrder.Customers.Customers.Fields.Spent")]
        public string Spent { get; set; }

        public bool IsSubscribedForEmail { get; set; }

        public bool IsSubscribedForSms { get; set; }

        public AddressModel DefaultShippingAddress { get; set; }

        public AddressModel DefaultBillingAddress { get; set; }

        public DateTime? SubscribedOn { get; set; }

        public string CustomerFromCreatedString { get; set; }
        
        #endregion

    }
}
