using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Utilities;
using Nop.Core.Domain.Customization.Custom.StoreWideDiscount;
using Nop.Web.Areas.Admin.Models.Customization.Utilities;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories.Customization
{
    public partial interface IUtilitiesModelFactory
    {
        Task<ProductNotesManagementModel> PrepareProductNotesManagementModel(ProductNotesManagementModel model);

        #region Bulk Update
        Task BulkUpdateInventory(string productids, int inventory);
        Task BulkUpdateProductCategorySpecificationAttributeMapping(string productids, string categoryIds, string specificationAttributeIds, bool isRemove);
        Task BulkPublishUnpublishProductCategories(string productids, string categoryIds, string specificationAttributeIds, bool published);
        Task BulkUpdateProductListing(string productIds, int productId, string ProductListingTypes);
        Task BulkUpdateTemplate(string categoryIds, int productTemplateId);
        Task<MarketingModel> PrepareMarketingModel();
        Task SaveMarketingSettings(MarketingModel model);
        Task<StoreWideDiscountSettingModel> PrepareOfferModel();
        Task BulkUpdateCustomizationFormTemplates(string productids, string categoryIds, int customizationFormTemplateId);

        #endregion

        #region  StoreWideDiscount
        Task<StoreWideDiscountSearchModel> PrepareStoreWideDiscountSearchModel(StoreWideDiscountSearchModel model);
        Task<StoreWideDiscountListModel> PrepareStoreWideDiscountListModelAsync(StoreWideDiscountSearchModel searchModel);
        Task<StoreWideDiscountModel> PrepareStoreWideDiscountModel(StoreWideDiscountModel model, StoreWideDiscount storeWideDiscount);
        #endregion

        #region  StoreWideDiscountSetting
        Task<StoreWideDiscountSettingListModel> PrepareStoreWideDiscountSettingListModelAsync(StoreWideDiscountSettingSearchModel searchModel);
        Task<StoreWideDiscountSettingModel> PrepareStoreWideDiscountSettingModel(StoreWideDiscountSettingModel model, StoreWideDiscountSetting storeWideDiscountSetting);
        #endregion

        #region  StoreWideProductDiscountInfo
        Task<StoreWideProductDiscountInfoSearchModel> PrepareOfferDiscountLogSearchModel(StoreWideProductDiscountInfoSearchModel model);
        Task<StoreWideProductDiscountInfoListModel> PrepareOfferDiscountListModelAsync(StoreWideProductDiscountInfoSearchModel searchModel);

        #endregion

        #region  StoreWideProductDiscountHistory
        Task<StoreWideProductDiscountHistorySearchModel> PrepareProductDiscountHistorySearchModel(StoreWideProductDiscountHistorySearchModel model);
        Task<StoreWideProductDiscountHistoryListModel> PrepareOfferProductDiscountHistoryListModelAsync(StoreWideProductDiscountHistorySearchModel searchModel);
        #endregion




        #region Testimonials

        Task<TestimonialListModel> SearchTestimonials(TestimonialSearchModel searchModel);

        #endregion  



    }
}
