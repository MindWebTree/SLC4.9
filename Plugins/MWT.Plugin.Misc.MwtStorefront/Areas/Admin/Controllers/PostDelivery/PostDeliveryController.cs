using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using System.Threading.Tasks;
using System;
using Nop.Web.Areas.Admin.Models.Customization.Custom.PostDelivery;
using Nop.Web.Areas.Admin.Factories.Customization.PostDelivery;
using Nop.Services.Customizations.Custom.PostPurchaseEmail;

namespace Nop.Web.Areas.Admin.Controllers
{
    public class PostDeliveryController : BaseAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IPostDeliveryQueueEmailModelFactory _postDeliveryQueueEmailModelFactory;
        private readonly IPostDeliveryService _postDeliveryService;

        #endregion

        #region Ctor

        public PostDeliveryController(IPermissionService permissionService, IPostDeliveryQueueEmailModelFactory postDeliveryQueueEmailModelFactory,
            IPostDeliveryService postDeliveryService)
        {
            _permissionService = permissionService;
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
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> PostDeliveryQueueEmailList(PostDeliveryQueueEmailSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _postDeliveryQueueEmailModelFactory.PreparePostDeliveryQueueEmailListModelAsync(searchModel);

            return Json(model);
        }


        [HttpPost]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> PostDeliveryQueueEmailUpdate(PostDeliveryQueueEmailModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

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
