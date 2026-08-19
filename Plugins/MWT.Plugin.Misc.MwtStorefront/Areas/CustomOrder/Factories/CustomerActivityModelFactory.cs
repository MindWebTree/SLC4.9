using Nop.Services.Logging;
using MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Models.Customers;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.CustomOrder.Factories
{
    public partial class CustomerActivityModelFactory : ICustomerActivityModelFactory
    {

        #region Fields

        private readonly ICustomerActivityService _customerActivityService;

        #endregion

        #region Ctor

        public CustomerActivityModelFactory(ICustomerActivityService customerActivityService)
        {
            this._customerActivityService = customerActivityService;
        }

        #endregion

        #region Methods

        public async Task<List<CustomerActivityModel>> PrepareCustomerActivityListModel(int customerID)
        {
     
            var activities = await _customerActivityService.GetAllActivitiesAsync(customerId: customerID);

            return activities.Select(activity =>
            {
                CustomerActivityModel model = new CustomerActivityModel();
                model.Details = activity.Comment;
                return model;
            }).ToList();
        }



        #endregion
    }
}
