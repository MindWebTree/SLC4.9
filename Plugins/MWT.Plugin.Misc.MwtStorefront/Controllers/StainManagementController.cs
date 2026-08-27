using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Services.Media;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Factories;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Mvc.Filters;


namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class StainManagementController : BaseAdminController
    {
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeExtendedModelFactory _productAttributeModelFactory;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        public StainManagementController(IProductAttributeService productAttributeService,
            IProductAttributeExtendedModelFactory productAttributeModelFactory , ICustomerActivityService customerActivityService,
            INotificationService notificationService, IPermissionService permissionService, ILocalizationService localizationService)
        {
            _productAttributeModelFactory = productAttributeModelFactory;
            _productAttributeService = productAttributeService;
            _customerActivityService = customerActivityService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _localizationService = localizationService;
        }
        public virtual async Task<IActionResult> Stains()
        {
            var _settingService = EngineContext.Current.Resolve<ISettingService>();
            var stainAttrid = await _settingService.GetSettingByKeyAsync<int>("catalog.product.attribute.stain.Id");
            var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(stainAttrid);
            if (productAttribute == null)
                return RedirectToAction("List");

            var model = new ProductAttributeModel();
            model.PredefinedProductAttributeValueSearchModel.ProductAttributeId = stainAttrid;
            return View(model);
        }

        [HttpPost]
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGESTAINS)]
        public virtual async Task<IActionResult> Stains(PredefinedProductAttributeValueSearchModel searchModel)
        { 
            //try to get a product attribute with the specified id
            var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(searchModel.ProductAttributeId)
                ?? throw new ArgumentException("No product attribute found with the specified id");

            //prepare model
            var model = await _productAttributeModelFactory.Stains(searchModel, productAttribute);

            return Json(model);
        }


        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGESTAINS)]
        public virtual async Task<IActionResult> StainsEdit(string stain)
        { 
            var _fileProvider = EngineContext.Current.Resolve<INopFileProvider>();
            PredefinedProductAttributeValueModel model = new PredefinedProductAttributeValueModel();
            model.Name = stain;
            var imagePath = Path.Combine("wwwroot", "images", "shades", "large", $"{stain}.jpg");
            var fileInfo = _fileProvider.GetFileInfo(imagePath);

            if (fileInfo.Exists)
            {
                DateTime date = DateTime.Now;
                model.Image = $"~/images/shades/large/{stain}.jpg?Date={date.ToString("yyyy-MM-ddTHH:mm:ss")}";
            }
            else
            {
                model.Image = "NA";
            }
            return View(model);
        }

        [HttpPost] 
        [CheckPermission(StandardPermission.CustomPermission.CUSTOM_ACCESS_MANAGESTAINS)]
        public virtual async Task<IActionResult> StainsEdit(IFormFile uploadedFile, string stain)
        { 

            var _fileProvider = EngineContext.Current.Resolve<INopFileProvider>();
            var _pictureService = EngineContext.Current.Resolve<IPictureExtendedService>();
            var _downloadService = EngineContext.Current.Resolve<IDownloadService>();
            PredefinedProductAttributeValueModel model = new PredefinedProductAttributeValueModel();
            model.Name = stain;
            var imagePath = Path.Combine("wwwroot", "images", "shades", "large", $"{stain}.jpg");
            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                var binary = await _downloadService.GetDownloadBitsAsync(uploadedFile);
                await _pictureService.SaveStainImage(binary, uploadedFile.ContentType, $"{stain}.jpg");
            }
            ViewBag.RefreshPage = true;

            return View(model);
        }

  
    }
}
