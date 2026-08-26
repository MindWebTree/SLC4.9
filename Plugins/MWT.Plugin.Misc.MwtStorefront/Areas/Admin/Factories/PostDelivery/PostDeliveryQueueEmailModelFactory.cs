using Nop.Services.Customers;
using Nop.Services.Customizations.Custom.PostPurchaseEmail;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Customization.Custom.PostDelivery;
using Nop.Web.Framework.Models.Extensions;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories.Customization.PostDelivery
{
    public partial class PostDeliveryQueueEmailModelFactory : IPostDeliveryQueueEmailModelFactory
    {

        #region fields

        private readonly IPostDeliveryService _postDeliveryService;
        private readonly ICustomerService _customerService;


        #endregion

        #region Ctor

        public PostDeliveryQueueEmailModelFactory(IPostDeliveryService postDeliveryService,
            ICustomerService customerService)
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
            var model = await new PostDeliveryQueueEmailListModel().PrepareToGridAsync(searchModel, queuedEmails,  () =>
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
