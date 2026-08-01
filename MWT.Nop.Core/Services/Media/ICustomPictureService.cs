using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Media;

namespace MWT.Nop.Core.Services.Media
{
    /// <summary>
    /// Picture service
    /// </summary>
    public partial interface ICustomPictureService : IPictureService
    { 
        Task<Picture> GetproductListingimage(int productId, bool isCategoryPage);
        Task<(int displayOnCategoryPagePictureId, int displayOnListingModulesPictureId, List<Picture> pictures)> CustomGetPicturesByProductIdAsync(int productId, int recordsToReturn = 0);
        Task<IList<CustomPicture>> CustomGetPicturesOfProducWithDimensionImageAsync(int productId, int recordsToReturn = 0);
    }
}
