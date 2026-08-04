using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Domain.QA;
using Nop.Data.Mapping;

namespace MWT.Plugin.Misc.MwtStorefront.Infrastructure.Mapping
{
    public   class CustomBaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {   
            //{ typeof(CustomOrder), "CustomOrder_Orders" },
        //     { typeof(CustomOrderCoupon), "CustomOrder_Coupon" },
        //     { typeof(CustomOrderCouponLog), "CustomOrder_Coupon_Log" },
        //     { typeof(CustomOrderNotesLog), "CustomOrder_Notes_Log" },
        //     { typeof(CustomOrderOrderLog), "CustomOrder_Order_Log" },
        //     { typeof(CustomorderOrderStatusLog), "CustomOrder_OrderStatus_Log" },
        //    { typeof(CustomOrderOrderSummaryAdjustmentLog), "CustomOrder_OrderSummaryAdjustment_Log" },
        //     { typeof(CustomOrderOrderType), "CustomOrder_OrderType" },
        //    { typeof(CustomOrderPriceAdjustment), "CustomOrder_PriceAdjustment" },
        //   { typeof(CustomOrderPriceAdjustmentLog), "CustomOrder_PriceAdjustment_Log" },
        //   { typeof(CustomOrderShoppingCartItem), "CustomOrder_ShoppingCartItem" },
        //{ typeof(CustomOrderStatus), "CustomOrder_Status" },
        // { typeof(CustomOrderOrderSummaryAdjustment), "CustomOrder_OrderSummaryAdjustment" },
        //        { typeof(CategoryCollectionLink), "Category_CollectionLink" },
                { typeof(CustomForm), "Custom_Forms" },
        //         { typeof(ProductSuggestedKeyword), "Product_SuggestedKeyword_Mapping" },
        //         { typeof(LogProductPicture), "Log_Product_Picture_Mapping" },
        //           { typeof(LogPicture), "Log_Picture" },
        //          { typeof(ProductKwTerm), "Product_KwTerm_Mapping" },
        //           { typeof(ProductAttributeValueGalleryPicturesMapping), "ProductAttributeValue_GalleryPictures_Mapping" },
        //            { typeof(CategorySuggestedKeyword), "Category_SuggestedKeyword_Mapping" },
                      { typeof(ProductQuestionAnswer), "Product_QuestionAnswer_Mapping" }
                               //{ typeof(CategoryUserMapping), "Category_User_Mapping" },
                               //{ typeof(ProductAttributeCombinationGalleryPicturesMapping), "ProductAttributeCombination_GalleryPictures_Mapping" },
                               //{ typeof(CategoryKwTerm), "Category_KwTerm_Mapping" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {
            { (typeof(CustomFormEntryMeta), "MetaKey"), "Meta_Key" },
             { (typeof(CustomFormEntryMeta), "MetaValue"), "Meta_Value" }

        };
    }
}
