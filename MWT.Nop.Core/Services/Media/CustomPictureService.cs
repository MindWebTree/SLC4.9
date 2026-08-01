using Microsoft.AspNetCore.Http;
using MWT.Nop.Core.Services.Seo;
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
using Picture = Nop.Core.Domain.Media.Picture;

namespace MWT.Nop.Core.Services.Media
{
    /// <summary>
    /// Picture service
    /// </summary>
    public partial  class CustomPictureService : PictureService, ICustomPictureService
    {
 
        public CustomPictureService(IDownloadService downloadService, IHttpContextAccessor httpContextAccessor, ILogger logger, INopFileProvider fileProvider, IProductAttributeParser productAttributeParser, IProductAttributeService productAttributeService, IRepository<Picture> pictureRepository, IRepository<PictureBinary> pictureBinaryRepository, IRepository<ProductPicture> productPictureRepository, ISettingService settingService, IThumbService thumbService, IUrlRecordService urlRecordService, IWebHelper webHelper, MediaSettings mediaSettings) : base(downloadService, httpContextAccessor, logger, fileProvider, productAttributeParser, productAttributeService, pictureRepository, pictureBinaryRepository, productPictureRepository, settingService, thumbService, urlRecordService, webHelper, mediaSettings)
        {
 
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
        public virtual async Task<(int displayOnCategoryPagePictureId, int displayOnListingModulesPictureId, List<Picture> pictures)> CustomGetPicturesByProductIdAsync(int productId, int recordsToReturn = 0)
        {
            if (productId == 0)
                return (0, 0, new List<Picture>());

            var query = from p in _pictureRepository.Table
                        join pp in _productPictureRepository.Table on p.Id equals pp.PictureId
                        orderby pp.DisplayOrder, pp.Id
                        where pp.ProductId == productId
                        select new
                        {
                            Id = p.Id,
                            SeoFilename = p.SeoFilename,
                            TitleAttribute = p.TitleAttribute,
                            VirtualPath = p.VirtualPath,
                            AltAttribute = p.AltAttribute,
                            IsNew = p.IsNew,
                            MimeType = p.MimeType,
                            DisplayOnListingModules = pp.DisplayOnListingModules,
                            DisplayOnCategoryPage = pp.DisplayOnCategoryPage

                        };

            if (recordsToReturn > 0)
                query = query.Take(recordsToReturn);

            var pics = await query.ToListAsync();

            int displayOnListingModulesPictureId = 0;
            int displayOnCategoryPagePictureId = 0;
            List<Picture> pictures = new List<Picture>();

            foreach (var pic in pics)
            {
                Picture picture = new Picture();
                picture.Id = pic.Id;
                picture.SeoFilename = pic.SeoFilename;
                picture.TitleAttribute = pic.TitleAttribute;
                picture.VirtualPath = pic.VirtualPath;
                picture.AltAttribute = pic.AltAttribute;
                picture.IsNew = pic.IsNew;
                picture.MimeType = pic.MimeType;
                pictures.Add(picture);

                if (pic.DisplayOnCategoryPage != null && Convert.ToBoolean(pic.DisplayOnCategoryPage))
                    displayOnCategoryPagePictureId = pic.Id;
                if (pic.DisplayOnListingModules != null && Convert.ToBoolean(pic.DisplayOnListingModules))
                    displayOnListingModulesPictureId = pic.Id;
            }

            return (displayOnCategoryPagePictureId, displayOnListingModulesPictureId, pictures);
        }
        public virtual async Task<IList<CustomPicture>> CustomGetPicturesOfProducWithDimensionImageAsync(int productId, int recordsToReturn = 0)
        {
            if (productId == 0)
                return new List<CustomPicture>();

            var query = from p in _pictureRepository.Table
                        join pp in _productPictureRepository.Table on p.Id equals pp.PictureId
                        orderby pp.DisplayOrder, pp.Id
                        where pp.ProductId == productId
                        && (pp.HideOnProductPage == false || pp.HideOnProductPage == null || pp.IsDimensionImage)
                        select new CustomPicture() { AltAttribute = p.AltAttribute, IsDimensionImage = pp.IsDimensionImage, Id = p.Id, IsNew = p.IsNew, MimeType = p.MimeType, SeoFilename = p.SeoFilename, TitleAttribute = p.TitleAttribute, VirtualPath = p.VirtualPath };

            if (recordsToReturn > 0)
                query = query.Take(recordsToReturn);

            var pics = await query.ToListAsync();

            return pics;
        }
    }
}
