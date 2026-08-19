using FluentValidation;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Validators.Customers
{
    public partial class CustomOrderCustomerSectionModelValidator : BaseNopValidator<CustomOrderCustomerSectionModel>
    {
        public CustomOrderCustomerSectionModelValidator(ILocalizationService localizationService,
            INopDataProvider dataProvider, ICustomerService customerService, AddressSettings addressSettings)
        {
            RuleFor(x => x.Email).NotEmpty()
                .EmailAddress()
                .WithMessage("Email Address required")
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.Common.WrongEmail"))
            //only for registered users
                .WhenAwait(async x => await IsRegisteredCustomerRoleCheckedAsync(x, customerService));

            RuleFor(x => x.FirstName)
                 .NotEmpty()
                 .WithMessageAwait(localizationService.GetResourceAsync("CustomOrder.Customers.Customers.Fields.FirstName.Required"));

            RuleFor(x => x.LastName)
                 .NotEmpty()
                 .WithMessageAwait(localizationService.GetResourceAsync("CustomOrder.Customers.Customers.Fields.FirstName.Required"));


    //        RuleFor(model => model.ShippingAddress)
    //.NotNull()
    //.SetValidator(new AddressValidator(addressSettings, localizationService, dataProvider));

    //        RuleFor(model => model.ShippingAddress)
    //     .NotNull()
    //     .SetValidator(new AddressValidator());

        }
        private async Task<bool> IsRegisteredCustomerRoleCheckedAsync(CustomOrderCustomerSectionModel model, ICustomerService customerService)
        {
            if (model.Id != 0)
            {
                var customer = await customerService.GetCustomerByIdAsync(model.Id);
                var roles = await customerService.GetCustomerRoleIdsAsync(customer, false);
                var allCustomerRoles = await customerService.GetAllCustomerRolesAsync(true);
                var newCustomerRoles = new List<CustomerRole>();
                foreach (var customerRole in allCustomerRoles)
                    if (roles.Contains(customerRole.Id))
                        newCustomerRoles.Add(customerRole);

                var isInRegisteredRole = newCustomerRoles.FirstOrDefault(cr => cr.SystemName == NopCustomerDefaults.RegisteredRoleName) != null;
                return isInRegisteredRole;
            }
            else
                return false;
        }

    }
}