using AutoMapper;
using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Domain.Custom.Campaign_Management;
using MWT.Nop.Core.Domain.Custom.LandingPage_Management;
using MWT.Nop.Core.Domain.QA;
using MWT.Plugin.Misc.MwtStorefront.Area.Admin.Models.Catalog.LandingPage_Management;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.Campaign_Management;
using MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models.QA;
using Nop.Core.Infrastructure.Mapper;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Infrastructure
{
    /// <summary>
    /// AutoMapper configuration for admin area models
    /// </summary>
    public class CustomMapperConfiguration : Profile, IOrderedMapperProfile
    {
        public CustomMapperConfiguration()
        {
            CreateCustomFormMaps();
            CreateCampaignManagemenMap();
            //CreateProductMap();

            //CreateStoreWideDiscountMap();

            //CreateKwtermMap();
            CreateLandingPageMap();
            CreateQuestionAnswerMap();
      //      CreateCategoryPermissionMap();

        }
        #region Utilities
        //     protected virtual void CreateStoreWideDiscountMap()
        //     {
        //         CreateMap<StoreWideDiscount, StoreWideDiscountModel>();
        //         CreateMap<StoreWideDiscountModel, StoreWideDiscount>();

        //         CreateMap<StoreWideDiscountSetting, StoreWideDiscountSettingModel>();
        //         CreateMap<StoreWideDiscountSettingModel, StoreWideDiscountSetting>();

        //         CreateMap<StoreWideProductDiscountInfo, StoreWideProductDiscountInfoModel>();
        //         CreateMap<StoreWideProductDiscountInfoModel, StoreWideProductDiscountInfoModel>();

        //         CreateMap<StoreWideProductDiscountHistory, StoreWideProductDiscountHistoryModel>();
        //         CreateMap<StoreWideProductDiscountHistoryModel, StoreWideProductDiscountHistory>();
        //     }
        protected virtual void CreateCustomFormMaps()
        {
            CreateMap<CustomForm, CustomFormModel>();
            CreateMap<CustomFormModel, CustomForm>();

            CreateMap<CustomFormEntry, CustomFormEntryModel>();
            CreateMap<CustomFormEntryModel, CustomFormEntry>();

            CreateMap<CustomFormEntryMeta, CustomFormEntryMetaModel>();
            CreateMap<CustomFormEntryMetaModel, CustomFormEntryMeta>();
        }

        //     public virtual void CreateProductMap()
        //     {
        //         CreateMap<LogProductPicture, LogProductPictureModel>()
        //.ForMember(model => model.OverrideAltAttribute, options => options.Ignore())
        //.ForMember(model => model.OverrideTitleAttribute, options => options.Ignore())
        //.ForMember(model => model.PictureUrl, options => options.Ignore());

        //     }
        public virtual void CreateCampaignManagemenMap()
        {
            CreateMap<MWT_CampaignTemplates, DesignModel>();
            CreateMap<DesignModel, MWT_CampaignTemplates>();
        }

        //     public virtual void CreateKwtermMap()
        //     {
        //         CreateMap<ProductKwTerm, KwTermProductModel>();
        //         CreateMap<KwTermProductModel, ProductKwTerm>();
        //         CreateMap<KwTermCategoryModel, CategoryKwTerm>();
        //         CreateMap<CategoryKwTerm, KwTermCategoryModel>();
        //     }
        public virtual void CreateLandingPageMap()
        {
            CreateMap<LandingPage, LandingPageModel>();
            CreateMap<LandingPageModel, LandingPage>();
        }
        public virtual void CreateQuestionAnswerMap()
        {
            CreateMap<ProductQuestionAnswer, QuestionAnswerProductModel>();
            CreateMap<QuestionAnswerProductModel, ProductQuestionAnswer>();
        }

        //public virtual void CreateCategoryPermissionMap()
        //{
        //    CreateMap<CategoryUserMapping, CategoryPermissionModel>();
        //    CreateMap<CategoryPermissionModel, CategoryUserMapping>();
        //}
        #endregion

        #region Properties

        /// <summary>
        /// Order of this mapper implementation
        /// </summary>
        public int Order => 0;

        #endregion
    }
}