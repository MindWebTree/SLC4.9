using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Domain.Catalog;
using MWT.Nop.Core.Domain.Media;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Media;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Services.Security;
using Nop.Web.Framework.Mvc.Filters;


namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class CommonController : BaseAdminController
    {
        [HttpPost]
        [CheckPermission(StandardPermission.Catalog.PRODUCTS_CREATE_EDIT_DELETE)]
        public virtual async Task<IActionResult> BulkUploadImages(int productId, string producyType, IFormCollection images)
        { 
            var _pictureService = EngineContext.Current.Resolve<IPictureExtendedService>();
            var _productService = EngineContext.Current.Resolve<IProductExtendedService>();
            switch (producyType)
            {
                case "Product":
                    var product = await _productService.GetProductByIdAsync(productId);
                    if (product != null)
                    {
                        var pictures = (await _productService.GetProductPicturesByProductIdAsync(productId));
                        var displayOrder = pictures.Count > 0 ? pictures.Max(p => p.DisplayOrder) : 0;
                        foreach (var file in images.Files)
                        {
                            var picture = await _pictureService.InsertPictureAsync(file, file.FileName);
                            if (picture != null)
                            {
                                displayOrder++;
                                await _pictureService.UpdatePictureAsync(picture.Id, await _pictureService.LoadPictureBinaryAsync(picture),
                                                                    picture.MimeType,
                                                                    picture.SeoFilename,
                                                                    "",
                                                                    "");
                                await _pictureService.SetSeoFilenameAsync(picture.Id, await _pictureService.GetPictureSeNameAsync(product.Name));
                                var productPicture = new ProductPicture
                                {
                                    PictureId = picture.Id,
                                    ProductId = productId,
                                    DisplayOrder = displayOrder,
                                    DisplayOnListingModules = false,
                                    HideOnProductPage = false,
                                    DisplayOnCategoryPage = false
                                };
                                await _productService.InsertProductPictureAsync(productPicture);



                                #region Log Picture
                                var pictureLog = new LogPicture()
                                {
                                    AltAttribute = picture.AltAttribute,
                                    IsNew = picture.IsNew,
                                    MimeType = picture.MimeType,
                                    ReferenceId = picture.Id,
                                    SeoFilename = picture.SeoFilename,
                                    TitleAttribute = picture.TitleAttribute,
                                    VirtualPath = picture.VirtualPath
                                };
                                await _pictureService.InsertPictureLog(pictureLog);

                                await _pictureService.InsertPictureMappingLog(new LogProductPicture()
                                {
                                    CreatedOn = DateTime.Now,
                                    CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                                    Action = "Add",
                                    ReferenceId = productPicture.Id,
                                    DisplayOnCategoryPage = false,
                                    DisplayOnListingModules = false,
                                    HideOnProductPage = false,
                                    ProductId = productId,
                                    PictureId = pictureLog.Id,
                                    DisplayOrder = displayOrder
                                });
                                #endregion
                            }
                        }
                    }
                    break;
                case "Default":
                    break;
            }
            return Json(new { Message = string.Format(await _localizationService.GetResourceAsync("Bulk.Upload.SucessMessage"), producyType) });

        }
    }
}
