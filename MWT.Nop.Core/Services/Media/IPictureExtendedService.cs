using Microsoft.AspNetCore.Http;
using MWT.Nop.Core.Domain.Catalog;
using MWT.Nop.Core.Domain.Media;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Media;

namespace MWT.Nop.Core.Services.Media
{
    /// <summary>
    /// Picture service
    /// </summary>
    public partial interface IPictureExtendedService : IPictureService
    {
        Task<Picture> GetproductListingimage(int productId, bool isCategoryPage);
        Task<IList<Picture>> CustomGetPicturesOfProducAsync(int productId, int recordsToReturn = 0);
        Task<(int displayOnCategoryPagePictureId, int displayOnListingModulesPictureId, List<Picture> pictures)> CustomGetPicturesByProductIdAsync(int productId, int recordsToReturn = 0);
        Task<IList<CustomPicture>> CustomGetPicturesOfProducWithDimensionImageAsync(int productId, int recordsToReturn = 0);
        Task SaveStainImage(byte[] pictureBinary, string mimeType, string fileName);
        Task DeletePictureExcludeImagesAsync(Picture picture);
        Task DeletePictureImagesAsync(Picture picture);
        Task<Picture> UpdatePictureAsync(int pictureId, IFormFile formFile, string defaultFileName = "", string virtualPath = "");
        Task<List<Picture>> GetProductAttributePicturesAsync(Product product, string attributesXml);


        #region Picture log
        Task InsertPictureLog(LogPicture picture);
        Task DeletePictureLog(LogPicture picture);
        Task<LogPicture> GetPictureLogById(int id);
        Task<List<LogPicture>> GetPictureLogsByRefrenceId(int referenceId);
        Task InsertPictureMappingLog(LogProductPicture logProductpicture);
        Task DeletePictureMappingLog(LogProductPicture logProductpicture);
        Task<List<LogProductPicture>> GetPictureMappingLogsByRefrenceId(int referenceId);
        Task<List<LogProductPicture>> GetPictureMappingLogsByPictureId(int pictureId);
        Task<List<LogProductPicture>> GetPictureMappingLogs(int productId, int displayOrder, int pictureId);
        Task<List<LogProductPicture>> GetPictureMappingLogsByProductId(int ProductId);
        #endregion
    }
}
