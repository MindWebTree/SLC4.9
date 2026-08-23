using Nop.Services.Tax;
using static Nop.Web.Models.ShoppingCart.OrderTotalsModel;
namespace MWT.Plugin.Misc.MwtStorefront.Models.Checkout;

public partial record ZipCodeTaxRateModel
{
    public string Tax { get; set; }
    public List<TaxInfo> Taxes { get; set; } = new List<TaxInfo>();
    public string ZipCode { get; set; }

    public IList<TaxRate> TaxRates { get; set; } = new List<TaxRate>();
}