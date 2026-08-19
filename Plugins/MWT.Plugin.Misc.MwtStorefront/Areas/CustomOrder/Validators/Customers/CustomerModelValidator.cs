
using FluentValidation;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.News;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Seo;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Customers;
using Nop.Web.Framework.Validators;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Validators.Customers
{
    public partial class CustomerModelValidator : BaseNopValidator<CustomerInfoModel>
    {
        public CustomerModelValidator(ILocalizationService localizationService,
            INopDataProvider dataProvider, ICustomerService customerService)
        {
            RuleFor(x => x.Email).NotEmpty()
                .EmailAddress()
                //.WithMessage("Valid Email is required for customer to be in 'Registered' role")
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.Common.WrongEmail"))
            //only for registered users
                .WhenAwait(async x => await IsRegisteredCustomerRoleCheckedAsync(x, customerService));

            RuleFor(x => x.FirstName)
                 .NotEmpty()
                 .WithMessageAwait(localizationService.GetResourceAsync("CustomOrder.Customers.Customers.Fields.FirstName.Required"));

            RuleFor(x => x.LastName)
                 .NotEmpty()
                 .WithMessageAwait(localizationService.GetResourceAsync("CustomOrder.Customers.Customers.Fields.FirstName.Required"));

            RuleFor(x => x.Phone)
                 .NotEmpty()
                 .WithMessageAwait(localizationService.GetResourceAsync("CustomOrder.Customers.Customers.Fields.Phone.Required"));

            RuleFor(x => x.ZipPostalCode)
           .NotEmpty()
           .WithMessageAwait(localizationService.GetResourceAsync("CustomOrder.Customers.Customers.Fields.Phone.Required"));

        }
        private async Task<bool> IsRegisteredCustomerRoleCheckedAsync(CustomerInfoModel model, ICustomerService customerService)
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