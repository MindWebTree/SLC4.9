using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Localization;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Orders;
using Nop.Web.Framework.Validators;



namespace MWT.Plugin.Misc.MwtStorefront.Areas.Validators.CustomOrder
{
    public partial class CustomOrderOrderTypeSectionModelvalidator : BaseNopValidator<CustomOrderOrderTypeSectionModel>
    {
        public CustomOrderOrderTypeSectionModelvalidator(ILocalizationService localizationService,
            INopDataProvider dataProvider, ICustomerService customerService)
        {
            //RuleFor(x => x.AlreadyFee)
                
            //    .WithMessageAwait(localizationService.GetResourceAsync("CustomOrder.OrderTypeSection.Fields.AlreadyFee.Decimal"));
            //RuleFor(x => x.HouzzFee)
               
            //         .WithMessageAwait(localizationService.GetResourceAsync("CustomOrder.OrderTypeSection.Fields.HouzzFee.Decimal"));

        }
     

    }
}

