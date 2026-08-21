using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Services.Orders;

namespace MWT.Nop.Core.Services.Orders
{
    public partial interface IOrderReportExtendedService: IOrderReportService
    {
        Task<IPagedList<BestsellersReportLine>> CustomBestSellersReportAsync(
            int categoryId = 0,
              int kwTermId = 0,
            int manufacturerId = 0,
            int storeId = 0,
            int vendorId = 0,
            DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null,
            OrderStatus? os = null,
            PaymentStatus? ps = null,
            ShippingStatus? ss = null,
            int billingCountryId = 0,
            OrderByEnum orderBy = OrderByEnum.OrderByQuantity,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool showHidden = false);
    }
}
