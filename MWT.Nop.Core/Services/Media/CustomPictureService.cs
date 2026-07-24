using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Seo;
using SkiaSharp;

namespace MWT.Nop.Core.Services.Media
{
    /// <summary>
    /// Picture service
    /// </summary>
    public partial class CustomPictureService : PictureService, IPictureService
    {
        private readonly IRepository<Picture> _pictureRepository;
        public CustomPictureService(IDownloadService downloadService, IHttpContextAccessor httpContextAccessor, ILogger logger, INopFileProvider fileProvider, IProductAttributeParser productAttributeParser, IProductAttributeService productAttributeService, IRepository<Picture> pictureRepository, IRepository<PictureBinary> pictureBinaryRepository, IRepository<ProductPicture> productPictureRepository, ISettingService settingService, IThumbService thumbService, IUrlRecordService urlRecordService, IWebHelper webHelper, MediaSettings mediaSettings) : base(downloadService, httpContextAccessor, logger, fileProvider, productAttributeParser, productAttributeService, pictureRepository, pictureBinaryRepository, productPictureRepository, settingService, thumbService, urlRecordService, webHelper, mediaSettings)
        {
            _pictureRepository = pictureRepository;
        }
 
        public virtual async Task<Picture> GetproductListingimage(int productId, bool isCategoryPage)
        {
            if (productId == 0)
                return new Picture();

            if (isCategoryPage)
            {
                var query = from p in _pictureRepository.Table
                             join pp in _productPictureRepository.Table on p.Id equals pp.PictureId
                             orderby pp.DisplayOrder, pp.Id
                             where pp.ProductId == productId
                             orderby pp.DisplayOnCategoryPage == null ? false : pp.DisplayOnCategoryPage descending, pp.DisplayOrder ascending
                             select p;
                query = query.Take(1);

                var pics = await query.ToListAsync();

                return pics.FirstOrDefault();
            }

            else
            {
                var query = from p in _pictureRepository.Table
                             join pp in _productPictureRepository.Table on p.Id equals pp.PictureId
                             orderby pp.DisplayOrder, pp.Id
                             where pp.ProductId == productId
                             orderby pp.DisplayOnListingModules == null ? false : pp.DisplayOnListingModules descending, pp.DisplayOrder ascending
                             select p;
                query = query.Take(1);

                var pics = await query.ToListAsync();

                return pics.FirstOrDefault();
            }
        }
         
    }
}
