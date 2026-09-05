using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Media;
using Nop.Services.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class CommonController : BaseAdminController
    {
        [HttpPost]
        public virtual async Task<IActionResult> BulkUploadImages(int productId, string producyType, IFormCollection images)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();
            var _pictureService = EngineContext.Current.Resolve<IPictureService>();
            var _productService = EngineContext.Current.Resolve<IProductService>();
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
                                var pictureLog = new Core.Domain.Media.LogPicture()
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
