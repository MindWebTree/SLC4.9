using System.Threading.Tasks;
using System;
using Nop.Web.Framework.Models.Extensions;
using Nop.Services.Catalog;
using System.Linq;
using Nop.Web.Areas.Admin.Models.Customization.Custom.Integrity_Report;
using Nop.Services.Media;

using Nop.Services.Customizations.Custom.Integrity_Report;

namespace Nop.Web.Areas.Admin.Factories.Customization.Integrity_Report
{
    public partial class IntegrityReportModelFactory : IIntegrityReportModelFactory
    {
        #region Fields

        private readonly IProductIntegrityReportService _productIntegrityReportService;
        private readonly IPictureService _pictureService;


        #endregion

        #region Ctor

        public IntegrityReportModelFactory(IProductIntegrityReportService productIntegrityReportService,
            IPictureService pictureService)
        {
            this._productIntegrityReportService = productIntegrityReportService;
            this._pictureService = pictureService;

        }

        #endregion



        public virtual async Task<ProductIntegrityReportListModel> PrepareProductIntegrityReportSearchListModelAsync(ProductIntegrityReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            var products = await _productIntegrityReportService.GetAllAsync();
            var pagedProducts = products.ToPagedList(searchModel);

            var model = await new ProductIntegrityReportListModel()
                .PrepareToGridAsync(searchModel, pagedProducts, () =>
                {
                    return pagedProducts.SelectAwait(async item =>
                    {
                        var data = new ProductIntegrityReportModel();

                        var defaultProductPicture =
                            (await _pictureService
                                .GetPicturesByProductIdAsync(item.ProductId, 1))
                            .FirstOrDefault();

                        (data.PictureThumbnailUrl, _) =
                            await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);

                        data.Id = item.ProductId;
                        data.Name = item.ProductName;
                        data.ErrorReason = item.ErrorReason;
                        return data;
                    });
                });

            return model;
        }


    }
}