using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class PictureController : BaseAdminController
    {
        #region Methods

        [HttpPost]
        //do not validate request token (XSRF)
        [IgnoreAntiforgeryToken]
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IActionResult> CustomAsyncUpload(int pictureId = 0)
        {
            //if (!await _permissionService.Authorize(StandardPermissionProvider.UploadPictures))
            //    return Json(new { success = false, error = "You do not have required permissions" }, "text/plain");
            var httpPostedFile = Request.Form.Files.FirstOrDefault();
            if (httpPostedFile == null)
                return Json(new { success = false, message = "No file uploaded" });

            const string qqFileNameParameter = "qqfilename";

            var qqFileName = Request.Form.ContainsKey(qqFileNameParameter)
                ? Request.Form[qqFileNameParameter].ToString()
                : string.Empty;
            if (pictureId == 0)
            {


                var pictureNew = await _pictureService.InsertPictureAsync(httpPostedFile, qqFileName);

                //when returning JSON the mime-type must be set to text/plain
                //otherwise some browsers will pop-up a "Save As" dialog.

                if (pictureNew == null)
                    return Json(new { success = false, message = "Wrong file format" });
                return Json(new
                {
                    success = true,
                    pictureId = pictureNew.Id,
                    imageUrl = (await _pictureService.GetPictureUrlAsync(pictureNew, 100)).Url
                });
            }
            else
            {

                var pictureupdated = await _pictureService.UpdatePictureAsync(pictureId, httpPostedFile, qqFileName);
                return Json(new
                {
                    success = true,
                    pictureId = pictureupdated.Id,
                    imageUrl = (await _pictureService.GetPictureUrlAsync(pictureupdated, 100)).Url+$"?updated={DateTime.Now.Ticks}"
                });
            }

        }
        #endregion
    }
}
