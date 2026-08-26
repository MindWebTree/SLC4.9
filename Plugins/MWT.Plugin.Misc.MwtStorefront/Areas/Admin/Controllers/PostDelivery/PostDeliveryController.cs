using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Services.PostDelivery;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Factories.PostDelievery;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Models.PostDelivery;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace MWT.Plugin.Misc.MwtStorefront.Area.Admin.Controllers.PostDelivery
{
    public class PostDeliveryController : BaseAdminController
    {
        #region Fields


        private readonly IPostDeliveryQueueEmailModelFactory _postDeliveryQueueEmailModelFactory;
        private readonly IPostDeliveryService _postDeliveryService;

        #endregion

        #region Ctor

        public PostDeliveryController(IPostDeliveryQueueEmailModelFactory postDeliveryQueueEmailModelFactory,
            IPostDeliveryService postDeliveryService)
        {
            _postDeliveryQueueEmailModelFactory = postDeliveryQueueEmailModelFactory;
            _postDeliveryService = postDeliveryService;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> Index()
        {
            return View(new PostDeliveryQueueEmailSearchModel());
        }

        [HttpPost]
        [CheckPermission(StandardPermission.Security.ACCESS_ADMIN_PANEL)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> PostDeliveryQueueEmailList(PostDeliveryQueueEmailSearchModel searchModel)
        {
            //prepare model
            var model = await _postDeliveryQueueEmailModelFactory.PreparePostDeliveryQueueEmailListModelAsync(searchModel);

            return Json(model);
        }


        [HttpPost]
        [CheckPermission(StandardPermission.Security.ACCESS_ADMIN_PANEL)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> PostDeliveryQueueEmailUpdate(PostDeliveryQueueEmailModel model)
        {
            //try to get a PairWith product with the specified id
            var postPurchaseEmailJourney = await _postDeliveryService.GetPostPurchaseEmailJourneyByOrderId(model.Id)
                ?? throw new ArgumentException("No Post Delivery QueueEmail found with the specified id");

            postPurchaseEmailJourney.IsActive = model.IsActive;
            postPurchaseEmailJourney.DeactivatedRemarks = model.DeactivatedRemarks;
            await _postDeliveryService.UpdatePostPurchaseEmailJourney(postPurchaseEmailJourney);

            return new NullJsonResult();
        }



        #endregion
    }
}
