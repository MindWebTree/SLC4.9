using MWT.Nop.Core.Services.Customers;
using MWT.Nop.Core.Services.PostDelivery;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Models.PostDelivery;
using Nop.Web.Framework.Models.Extensions;

namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories.PostDelievery
{
    public partial class PostDeliveryQueueEmailModelFactory : IPostDeliveryQueueEmailModelFactory
    {

        #region fields

        private readonly IPostDeliveryService _postDeliveryService;
        private readonly ICustomerExtendedService _customerService;


        #endregion

        #region Ctor

        public PostDeliveryQueueEmailModelFactory(IPostDeliveryService postDeliveryService,
            ICustomerExtendedService customerService)
        {
            _postDeliveryService = postDeliveryService;
            _customerService = customerService;
        }


        #endregion

        #region Methods
        public async Task<PostDeliveryQueueEmailListModel> PreparePostDeliveryQueueEmailListModelAsync(PostDeliveryQueueEmailSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));



            //get FBT products
            var queuedEmails = (await _postDeliveryService.GetPostDeliveryQueueEmailList()).ToPagedList(searchModel);

            //prepare grid model
            var model = await new PostDeliveryQueueEmailListModel().PrepareToGridAsync(searchModel, queuedEmails, () =>
            {

                return queuedEmails.SelectAwait(async queuedEmail =>
                {
                    var customer = await _customerService.GetCustomerByIdAsync(queuedEmail.OrderId);
                    //fill in model values from the entity
                    var PostDeliveryQueueEmailModel = new PostDeliveryQueueEmailModel();
                    PostDeliveryQueueEmailModel.Id = queuedEmail.OrderId;
                    PostDeliveryQueueEmailModel.ReminderDate = queuedEmail.ReminderDate;
                    PostDeliveryQueueEmailModel.IsActive = queuedEmail.IsActive;
                    PostDeliveryQueueEmailModel.DeactivatedRemarks = queuedEmail.DeactivatedRemarks;
                    PostDeliveryQueueEmailModel.ReminderNumber = queuedEmail.ReminderNumber;
                    PostDeliveryQueueEmailModel.Email = customer == null ? queuedEmail.Email : await _customerService.GetCustomerEmail(customer);
                    return PostDeliveryQueueEmailModel;
                });
            });
            return model;
        }
        #endregion

    }
}
