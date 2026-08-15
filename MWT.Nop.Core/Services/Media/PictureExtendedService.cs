using Microsoft.AspNetCore.Http;
using MWT.Nop.Core.Domain.Catalog;
using MWT.Nop.Core.Domain.Media;
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
    public partial class PictureExtendedService : PictureService, IPictureExtendedService
    {

        public PictureExtendedService(IDownloadService downloadService, IHttpContextAccessor httpContextAccessor, ILogger logger, INopFileProvider fileProvider, IProductAttributeParser productAttributeParser, IProductAttributeService productAttributeService, IRepository<Picture> pictureRepository, IRepository<PictureBinary> pictureBinaryRepository, IRepository<ProductPicture> productPictureRepository, ISettingService settingService, IThumbService thumbService, IUrlRecordService urlRecordService, IWebHelper webHelper, MediaSettings mediaSettings) : base(downloadService, httpContextAccessor, logger, fileProvider, productAttributeParser, productAttributeService, pictureRepository, pictureBinaryRepository, productPictureRepository, settingService, thumbService, urlRecordService, webHelper, mediaSettings)
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
        public virtual async Task<IList<Picture>> CustomGetPicturesOfProducAsync(int productId, int recordsToReturn = 0)
        {
            if (productId == 0)
                return new List<Picture>();

            var query = from p in _pictureRepository.Table
                        join pp in _productPictureRepository.Table on p.Id equals pp.PictureId
                        orderby pp.DisplayOrder, pp.Id
                        where pp.ProductId == productId
                        && (pp.HideOnProductPage == false || pp.HideOnProductPage == null)
                        select p;

            if (recordsToReturn > 0)
                query = query.Take(recordsToReturn);

            var pics = await query.ToListAsync();

            return pics;
        }
        public async Task SaveStainImage(byte[] pictureBinary, string mimeType, string fileName)
        {

            #region Shade Large Image

            using (var image = SKBitmap.Decode(pictureBinary))
            {

                //resize the image in accordance with the maximum size
                if (Math.Max(image.Height, image.Width) > 200)
                {
                    var format = GetImageFormatByMimeType(mimeType);
                    pictureBinary = ImageResize(image, format, 200);
                }
                var thumbsDirectoryPath = _fileProvider.GetAbsolutePath(@"images\shades\large");
                thumbsDirectoryPath = _fileProvider.Combine(thumbsDirectoryPath, fileName);
                await _thumbService.SaveThumbAsync(thumbsDirectoryPath, fileName, string.Empty, pictureBinary);
            }
            #endregion

            #region Shade small Image Image

            using (var image = SKBitmap.Decode(pictureBinary))
            {
                //resize the image in accordance with the maximum size
                if (Math.Max(image.Height, image.Width) > 44)
                {
                    var format = GetImageFormatByMimeType(mimeType);
                    pictureBinary = ImageResize(image, format, 44);
                }
                var thumbsDirectoryPath = _fileProvider.GetAbsolutePath(@"images\shades");
                thumbsDirectoryPath = _fileProvider.Combine(thumbsDirectoryPath, fileName);
                await _thumbService.SaveThumbAsync(thumbsDirectoryPath, fileName, string.Empty, pictureBinary);

                thumbsDirectoryPath = _fileProvider.GetAbsolutePath(@"images\shades\Squareimage");
                thumbsDirectoryPath = _fileProvider.Combine(thumbsDirectoryPath, fileName);
                await _thumbService.SaveThumbAsync(thumbsDirectoryPath, fileName, string.Empty, pictureBinary);
            }
            #endregion
        }


        public virtual async Task DeletePictureExcludeImagesAsync(Picture picture)
        {
            await _pictureRepository.DeleteAsync(picture);
        }

        public virtual async Task DeletePictureImagesAsync(Picture picture)
        {
            if (picture == null)
                throw new ArgumentNullException(nameof(picture));

            //delete thumbs
            await this._thumbService.DeletePictureThumbsAsync(picture);
        }


        public virtual async Task<Picture> UpdatePictureAsync(int pictureId, IFormFile formFile, string defaultFileName = "", string virtualPath = "")
        {
            var imgExt = new List<string>
            {
                ".bmp",
                ".gif",
                ".webp",
                ".jpeg",
                ".jpg",
                ".jpe",
                ".jfif",
                ".pjpeg",
                ".pjp",
                ".png",
                ".tiff",
                ".tif"
            } as IReadOnlyCollection<string>;

            var fileName = formFile.FileName;
            if (string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(defaultFileName))
                fileName = defaultFileName;

            //remove path (passed in IE)
            fileName = _fileProvider.GetFileName(fileName);

            var contentType = formFile.ContentType;

            var fileExtension = _fileProvider.GetFileExtension(fileName);
            if (!string.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();

            if (imgExt.All(ext => !ext.Equals(fileExtension, StringComparison.CurrentCultureIgnoreCase)))
                return null;

            //contentType is not always available 
            //that's why we manually update it here
            //http://www.sfsu.edu/training/mimetype.htm
            if (string.IsNullOrEmpty(contentType))
            {
                switch (fileExtension)
                {
                    case ".bmp":
                        contentType = MimeTypes.ImageBmp;
                        break;
                    case ".gif":
                        contentType = MimeTypes.ImageGif;
                        break;
                    case ".jpeg":
                    case ".jpg":
                    case ".jpe":
                    case ".jfif":
                    case ".pjpeg":
                    case ".pjp":
                        contentType = MimeTypes.ImageJpeg;
                        break;
                    case ".webp":
                        contentType = MimeTypes.ImageWebp;
                        break;
                    case ".png":
                        contentType = MimeTypes.ImagePng;
                        break;
                    case ".tiff":
                    case ".tif":
                        contentType = MimeTypes.ImageTiff;
                        break;
                    default:
                        break;
                }
            }
            var oldPicture = await GetPictureByIdAsync(pictureId);

            var picture = await UpdatePictureAsync(pictureId, await _downloadService.GetDownloadBitsAsync(formFile), contentType, oldPicture.SeoFilename, oldPicture.AltAttribute, oldPicture.TitleAttribute);

            if (string.IsNullOrEmpty(virtualPath))
                return picture;

            picture.VirtualPath = _fileProvider.GetVirtualPath(virtualPath);
            await UpdatePictureAsync(picture);

            return picture;
        }


        public virtual async Task<List<Picture>> GetProductAttributePicturesAsync(Product product, string attributesXml)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            List<Picture> pictures = new List<Picture>();

            var combination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributesXml);
            if (combination != null)
            {
                var combinationPictures = await _productAttributeService.GetProductAttributeCombinationPicturesAsync(((ProductAttributeCombination)combination).Id);

                if (combinationPictures.Any())
                {
                    var combinationPicture = await GetPictureByIdAsync(combinationPictures.First().PictureId);
                    if (combinationPicture != null)
                        pictures.Add(combinationPicture);
                }
            }

            var attributeValues = await _productAttributeParser
    .ParseProductAttributeValuesAsync(attributesXml);

            foreach (var attributeValue in attributeValues)
            {
                var attrValuePictures = await _productAttributeService.GetProductAttributeValuePicturesAsync(attributeValue.Id);
                if (attrValuePictures.Any())
                {
                    var attrValuePicture = await GetPictureByIdAsync(attrValuePictures.First().PictureId);
                    if (attrValuePicture != null)
                    {
                        if (!pictures.Any(pic => pic.Id == attrValuePicture.Id))
                            pictures.Add(attrValuePicture);

                    }
                }
            }

            return pictures;
        }

        #region Picture log
        public async Task InsertPictureLog(LogPicture picture)
        {
            var _logPictureRepository = EngineContext.Current.Resolve<IRepository<LogPicture>>();
            await _logPictureRepository.InsertAsync(picture);

        }
        public async Task DeletePictureLog(LogPicture picture)
        {
            var _logPictureRepository = EngineContext.Current.Resolve<IRepository<LogPicture>>();
            await _logPictureRepository.DeleteAsync(picture);
        }
        public async Task<LogPicture> GetPictureLogById(int id)
        {
            var _logPictureRepository = EngineContext.Current.Resolve<IRepository<LogPicture>>();
            return await _logPictureRepository.GetByIdAsync(id);
        }

        public async Task<List<LogPicture>> GetPictureLogsByRefrenceId(int referenceId)
        {
            var _logPictureRepository = EngineContext.Current.Resolve<IRepository<LogPicture>>();
            return await (from logPictueMapping in _logPictureRepository.Table
                          where logPictueMapping.ReferenceId == referenceId
                          select logPictueMapping).ToListAsync();

        }
        public async Task InsertPictureMappingLog(LogProductPicture logProductpicture)
        {
            var _logProductPictureRepository = EngineContext.Current.Resolve<IRepository<LogProductPicture>>();
            await _logProductPictureRepository.InsertAsync(logProductpicture);
        }
        public async Task DeletePictureMappingLog(LogProductPicture logProductpicture)
        {
            var _logProductPictureRepository = EngineContext.Current.Resolve<IRepository<LogProductPicture>>();
            await _logProductPictureRepository.DeleteAsync(logProductpicture);
        }
        public async Task<List<LogProductPicture>> GetPictureMappingLogs(int productId, int displayOrder, int pictureId)
        {
            var pictures = (await this.GetPicturesByProductIdAsync(productId)).Select(p => p.Id);
            var _logProductPictureRepository = EngineContext.Current.Resolve<IRepository<LogProductPicture>>();
            var _logPictureRepository = EngineContext.Current.Resolve<IRepository<LogPicture>>();

            List<LogProductPicture> logPictures = await (from lgPictureMapping in _logProductPictureRepository.Table
                                                         join logPicure in _logPictureRepository.Table on
                                                         lgPictureMapping.PictureId equals logPicure.Id
                                                         where lgPictureMapping.ProductId == productId
                                                         && logPicure.ReferenceId == pictureId
                                                         orderby lgPictureMapping.Id
                                                         select lgPictureMapping).ToListAsync();

            logPictures.AddRange(await (from lgPictureMapping in _logProductPictureRepository.Table
                                        join logPicure in _logPictureRepository.Table on
                                        lgPictureMapping.PictureId equals logPicure.Id
                                        where lgPictureMapping.ProductId == productId
                                        && lgPictureMapping.DisplayOrder == displayOrder
                                        && !pictures.Contains(logPicure.ReferenceId)
                                        && logPicure.ReferenceId < pictureId
                                        orderby lgPictureMapping.Id
                                        select lgPictureMapping).ToListAsync());

            return logPictures;



        }
        public async Task<List<LogProductPicture>> GetPictureMappingLogsByRefrenceId(int referenceId)
        {
            var _logProductPictureRepository = EngineContext.Current.Resolve<IRepository<LogProductPicture>>();
            return await (from logProductPictueMapping in _logProductPictureRepository.Table
                          where logProductPictueMapping.ReferenceId == referenceId
                          select logProductPictueMapping).ToListAsync();

        }

        public async Task<List<LogProductPicture>> GetPictureMappingLogsByPictureId(int pictureId)
        {
            var _logProductPictureRepository = EngineContext.Current.Resolve<IRepository<LogProductPicture>>();
            return await (from logProductPictueMapping in _logProductPictureRepository.Table
                          where logProductPictueMapping.PictureId == pictureId
                          select logProductPictueMapping).ToListAsync();
        }

        public async Task<List<LogProductPicture>> GetPictureMappingLogsByProductId(int ProductId)
        {
            var _logProductPictureRepository = EngineContext.Current.Resolve<IRepository<LogProductPicture>>();
            return await (from logProductPictueMapping in _logProductPictureRepository.Table
                          where logProductPictueMapping.ProductId == ProductId
                          orderby logProductPictueMapping.Id descending
                          select logProductPictueMapping).ToListAsync();
        }


        #endregion
    }
}
