using MWT.Nop.Core.Service.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    internal class CopyProductExtendedService : CopyProductService, ICopyProductExtendedService
    {
        private readonly IProductExtendedService _productExtendedService;
        private readonly ICustomProductAttributeService _customProductAttributeService;

        public CopyProductExtendedService(IAclService aclService, ICategoryService categoryService, IDownloadService downloadService, ILanguageService languageService, ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService, IManufacturerService manufacturerService, IPictureService pictureService, IProductAttributeParser productAttributeParser, IProductAttributeService productAttributeService,
            IProductService productService, IProductTagService productTagService, ISpecificationAttributeService specificationAttributeService, IStoreMappingService storeMappingService, IUrlRecordService urlRecordService, IVideoService videoService,
            IProductExtendedService productExtendedService, ICustomProductAttributeService customProductAttributeService)
            : base(aclService, categoryService, downloadService, languageService, localizationService, localizedEntityService, manufacturerService, pictureService, productAttributeParser, productAttributeService, productService, productTagService, specificationAttributeService, storeMappingService, urlRecordService, videoService)
        {
            _productExtendedService = productExtendedService;
            _customProductAttributeService = customProductAttributeService;
        }

        #region Methods
 

        public virtual async Task<Product> CustomCopyProductAsync(Product product, string newName, string newSku,
       bool isPublished = true, bool copyMultimedia = true, bool copyAssociatedProducts = true)
        {
            ArgumentNullException.ThrowIfNull(product);

            if (string.IsNullOrEmpty(newName))
                throw new ArgumentException("Product name is required");

            var productCopy = await CustomCopyBaseProductDataAsync(product, newName,newSku, isPublished);

            //localization
            await CopyLocalizationDataAsync(product, productCopy);

            //copy product tags
            foreach (var productTag in await _productTagService.GetAllProductTagsByProductIdAsync(product.Id))
                await _productTagService.InsertProductProductTagMappingAsync(new ProductProductTagMapping { ProductTagId = productTag.Id, ProductId = productCopy.Id });

            //copy product pictures
            var originalNewPictureIdentifiers = await CopyProductPicturesAsync(product, newName, copyMultimedia, productCopy);

            //copy product videos
            await CopyProductVideosAsync(product, copyMultimedia, productCopy);

            //quantity change history
            await _productService.AddStockQuantityHistoryEntryAsync(productCopy, product.StockQuantity, product.StockQuantity, product.WarehouseId,
                string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.CopyProduct"), product.Id));

            //product specifications
            await CopyProductSpecificationsAsync(product, productCopy);

            //product <-> warehouses mappings
            await CopyWarehousesMappingAsync(product, productCopy);
            //product <-> categories mappings
            await CopyCategoriesMappingAsync(product, productCopy);
            //product <-> manufacturers mappings
            await CopyManufacturersMappingAsync(product, productCopy);
            //product <-> related products mappings
            await CopyRelatedProductsMappingAsync(product, productCopy);
            //product <-> cross sells mappings
            await CopyCrossSellsMappingAsync(product, productCopy);
            //product <-> attributes mappings
            await CustomCopyAttributesMappingAsync(product, productCopy, originalNewPictureIdentifiers);
            //product <-> discounts mapping
            await CopyDiscountsMappingAsync(product, productCopy);

            //store mapping
            var selectedStoreIds = await _storeMappingService.GetStoresIdsWithAccessAsync(product);
            foreach (var id in selectedStoreIds)
                await _storeMappingService.InsertStoreMappingAsync(productCopy, id);

            //customer role mapping
            var customerRoleIds = await _aclService.GetCustomerRoleIdsWithAccessAsync(product.Id, nameof(Product));

            foreach (var id in customerRoleIds)
                await _aclService.InsertAclRecordAsync(productCopy, id);

            //tier prices
            await CopyTierPricesAsync(product, productCopy);

            //associated products
            await CopyAssociatedProductsAsync(product, isPublished, copyMultimedia, copyAssociatedProducts, productCopy);

            return productCopy;
        }

        #endregion

        #region Utilities

        protected virtual async Task<Product> CustomCopyBaseProductDataAsync(Product product, string newName, string newSku, bool isPublished)
        {
            //product download & sample download
            var downloadId = product.DownloadId;
            var sampleDownloadId = product.SampleDownloadId;
            if (product.IsDownload)
            {
                var download = await _downloadService.GetDownloadByIdAsync(product.DownloadId);
                if (download != null)
                {
                    var downloadCopy = new Download
                    {
                        DownloadGuid = Guid.NewGuid(),
                        UseDownloadUrl = download.UseDownloadUrl,
                        DownloadUrl = download.DownloadUrl,
                        DownloadBinary = download.DownloadBinary,
                        ContentType = download.ContentType,
                        Filename = download.Filename,
                        Extension = download.Extension,
                        IsNew = download.IsNew
                    };
                    await _downloadService.InsertDownloadAsync(downloadCopy);
                    downloadId = downloadCopy.Id;
                }

                if (product.HasSampleDownload)
                {
                    var sampleDownload = await _downloadService.GetDownloadByIdAsync(product.SampleDownloadId);
                    if (sampleDownload != null)
                    {
                        var sampleDownloadCopy = new Download
                        {
                            DownloadGuid = Guid.NewGuid(),
                            UseDownloadUrl = sampleDownload.UseDownloadUrl,
                            DownloadUrl = sampleDownload.DownloadUrl,
                            DownloadBinary = sampleDownload.DownloadBinary,
                            ContentType = sampleDownload.ContentType,
                            Filename = sampleDownload.Filename,
                            Extension = sampleDownload.Extension,
                            IsNew = sampleDownload.IsNew
                        };
                        await _downloadService.InsertDownloadAsync(sampleDownloadCopy);
                        sampleDownloadId = sampleDownloadCopy.Id;
                    }
                }
            }
            var _newSku = string.IsNullOrWhiteSpace((newSku ?? "").Trim()) ? (!string.IsNullOrWhiteSpace(product.Sku)
                          ? string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Copy.SKU.New"), product.Sku)
                          : product.Sku) : newSku;
            // product
            var productCopy = new Product
            {
                ProductTypeId = product.ProductTypeId,
                ParentGroupedProductId = product.ParentGroupedProductId,
                VisibleIndividually = product.VisibleIndividually,
                Name = newName,
                ShortDescription = product.ShortDescription,
                FullDescription = product.FullDescription,
                VendorId = product.VendorId,
                ProductTemplateId = product.ProductTemplateId,
                AdminComment = product.AdminComment,
                ShowOnHomepage = product.ShowOnHomepage,
                MetaKeywords = product.MetaKeywords,
                MetaDescription = product.MetaDescription,
                MetaTitle = product.MetaTitle,
                AllowCustomerReviews = product.AllowCustomerReviews,
                LimitedToStores = product.LimitedToStores,
                SubjectToAcl = product.SubjectToAcl,
                Sku = _newSku,
                ManufacturerPartNumber = product.ManufacturerPartNumber,
                Gtin = product.Gtin,
                IsGiftCard = product.IsGiftCard,
                GiftCardType = product.GiftCardType,
                OverriddenGiftCardAmount = product.OverriddenGiftCardAmount,
                RequireOtherProducts = product.RequireOtherProducts,
                RequiredProductIds = product.RequiredProductIds,
                AutomaticallyAddRequiredProducts = product.AutomaticallyAddRequiredProducts,
                IsDownload = product.IsDownload,
                DownloadId = downloadId,
                UnlimitedDownloads = product.UnlimitedDownloads,
                MaxNumberOfDownloads = product.MaxNumberOfDownloads,
                DownloadExpirationDays = product.DownloadExpirationDays,
                DownloadActivationType = product.DownloadActivationType,
                HasSampleDownload = product.HasSampleDownload,
                SampleDownloadId = sampleDownloadId,
                HasUserAgreement = product.HasUserAgreement,
                UserAgreementText = product.UserAgreementText,
                IsRecurring = product.IsRecurring,
                RecurringCycleLength = product.RecurringCycleLength,
                RecurringCyclePeriod = product.RecurringCyclePeriod,
                RecurringTotalCycles = product.RecurringTotalCycles,
                IsRental = product.IsRental,
                RentalPriceLength = product.RentalPriceLength,
                RentalPricePeriod = product.RentalPricePeriod,
                IsShipEnabled = product.IsShipEnabled,
                IsFreeShipping = product.IsFreeShipping,
                ShipSeparately = product.ShipSeparately,
                AdditionalShippingCharge = product.AdditionalShippingCharge,
                DeliveryDateId = product.DeliveryDateId,
                IsTaxExempt = product.IsTaxExempt,
                TaxCategoryId = product.TaxCategoryId,
                ManageInventoryMethod = product.ManageInventoryMethod,
                ProductAvailabilityRangeId = product.ProductAvailabilityRangeId,
                UseMultipleWarehouses = product.UseMultipleWarehouses,
                WarehouseId = product.WarehouseId,
                StockQuantity = product.StockQuantity,
                DisplayStockAvailability = product.DisplayStockAvailability,
                DisplayStockQuantity = product.DisplayStockQuantity,
                MinStockQuantity = product.MinStockQuantity,
                LowStockActivityId = product.LowStockActivityId,
                NotifyAdminForQuantityBelow = product.NotifyAdminForQuantityBelow,
                BackorderMode = product.BackorderMode,
                AllowBackInStockSubscriptions = product.AllowBackInStockSubscriptions,
                OrderMinimumQuantity = product.OrderMinimumQuantity,
                OrderMaximumQuantity = product.OrderMaximumQuantity,
                AllowedQuantities = product.AllowedQuantities,
                AllowAddingOnlyExistingAttributeCombinations = product.AllowAddingOnlyExistingAttributeCombinations,
                NotReturnable = product.NotReturnable,
                DisableBuyButton = product.DisableBuyButton,
                DisableWishlistButton = product.DisableWishlistButton,
                AvailableForPreOrder = product.AvailableForPreOrder,
                PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc,
                CallForPrice = product.CallForPrice,
                Price = product.Price,
                OldPrice = product.OldPrice,
                ProductCost = product.ProductCost,
                CustomerEntersPrice = product.CustomerEntersPrice,
                MinimumCustomerEnteredPrice = product.MinimumCustomerEnteredPrice,
                MaximumCustomerEnteredPrice = product.MaximumCustomerEnteredPrice,
                BasepriceEnabled = product.BasepriceEnabled,
                BasepriceAmount = product.BasepriceAmount,
                BasepriceUnitId = product.BasepriceUnitId,
                BasepriceBaseAmount = product.BasepriceBaseAmount,
                BasepriceBaseUnitId = product.BasepriceBaseUnitId,
                MarkAsNew = product.MarkAsNew,
                MarkAsNewStartDateTimeUtc = product.MarkAsNewStartDateTimeUtc,
                MarkAsNewEndDateTimeUtc = product.MarkAsNewEndDateTimeUtc,
                Weight = product.Weight,
                Length = product.Length,
                Width = product.Width,
                Height = product.Height,
                AvailableStartDateTimeUtc = product.AvailableStartDateTimeUtc,
                AvailableEndDateTimeUtc = product.AvailableEndDateTimeUtc,
                DisplayOrder = product.DisplayOrder,
                Published = isPublished,
                Deleted = product.Deleted,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                AgeVerification = product.AgeVerification,
                MinimumAgeToPurchase = product.MinimumAgeToPurchase
            };

            //validate search engine name
            await _productService.InsertProductAsync(productCopy);

            //search engine name
            await _urlRecordService.SaveSlugAsync(productCopy, await _urlRecordService.ValidateSeNameAsync(productCopy, string.Empty, productCopy.Name, true), 0);
            return productCopy;
        }
        protected virtual async Task CustomCopyAttributesMappingAsync(Product product, Product productCopy, Dictionary<int, int> originalNewPictureIdentifiers)
        {
            var associatedAttributes = new Dictionary<int, int>();
            var associatedAttributeValues = new Dictionary<int, int>();

            //attribute mapping with condition attributes
            var oldCopyWithConditionAttributes = new List<ProductAttributeMapping>();

            //all product attribute mapping copies
            var productAttributeMappingCopies = new Dictionary<int, ProductAttributeMapping>();

            var languages = await _languageService.GetAllLanguagesAsync(true);

            foreach (var productAttributeMapping in await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id))
            {
                var productAttributeMappingCopy = new ProductAttributeMapping
                {
                    ProductId = productCopy.Id,
                    ProductAttributeId = productAttributeMapping.ProductAttributeId,
                    TextPrompt = productAttributeMapping.TextPrompt,
                    IsRequired = productAttributeMapping.IsRequired,
                    AttributeControlTypeId = productAttributeMapping.AttributeControlTypeId,
                    DisplayOrder = productAttributeMapping.DisplayOrder,
                    ValidationMinLength = productAttributeMapping.ValidationMinLength,
                    ValidationMaxLength = productAttributeMapping.ValidationMaxLength,
                    ValidationFileAllowedExtensions = productAttributeMapping.ValidationFileAllowedExtensions,
                    ValidationFileMaximumSize = productAttributeMapping.ValidationFileMaximumSize,
                    DefaultValue = productAttributeMapping.DefaultValue,
                    EnableHoverImpact = productAttributeMapping.EnableHoverImpact,
                    IsExpanded = productAttributeMapping.IsExpanded
                };
                await _productAttributeService.InsertProductAttributeMappingAsync(productAttributeMappingCopy);
                //localization
                foreach (var lang in languages)
                {
                    var textPrompt = await _localizationService.GetLocalizedAsync(productAttributeMapping, x => x.TextPrompt, lang.Id, false, false);
                    if (!string.IsNullOrEmpty(textPrompt))
                        await _localizedEntityService.SaveLocalizedValueAsync(productAttributeMappingCopy, x => x.TextPrompt, textPrompt,
                            lang.Id);
                }

                productAttributeMappingCopies.Add(productAttributeMappingCopy.Id, productAttributeMappingCopy);

                if (!string.IsNullOrEmpty(productAttributeMapping.ConditionAttributeXml))
                {
                    oldCopyWithConditionAttributes.Add(productAttributeMapping);
                }

                //save associated value (used for combinations copying)
                associatedAttributes.Add(productAttributeMapping.Id, productAttributeMappingCopy.Id);

                // product attribute values
                var productAttributeValues = await _productAttributeService.GetProductAttributeValuesAsync(productAttributeMapping.Id);
                var attribute = await _productAttributeService.GetProductAttributeByIdAsync(productAttributeMapping.ProductAttributeId);
                foreach (var productAttributeValue in productAttributeValues)
                {
                    var attributeValueCopy = new ProductAttributeValue
                    {
                        ProductAttributeMappingId = productAttributeMappingCopy.Id,
                        AttributeValueTypeId = productAttributeValue.AttributeValueTypeId,
                        AssociatedProductId = productAttributeValue.AssociatedProductId,
                        Name = productAttributeValue.Name,
                        ColorSquaresRgb = productAttributeValue.ColorSquaresRgb,
                        PriceAdjustment = productAttributeValue.PriceAdjustment,
                        PriceAdjustmentUsePercentage = productAttributeValue.PriceAdjustmentUsePercentage,
                        WeightAdjustment = productAttributeValue.WeightAdjustment,
                        Cost = productAttributeValue.Cost,
                        CustomerEntersQty = productAttributeValue.CustomerEntersQty,
                        Quantity = productAttributeValue.Quantity,
                        IsPreSelected = productAttributeValue.IsPreSelected,
                        DisplayOrder = productAttributeValue.DisplayOrder,
                        Dimension = productAttributeValue.Dimension,
                        VariantDimension = productAttributeValue.VariantDimension
                    };

                    //picture
                    var oldValuePictures = await _productAttributeService.GetProductAttributeValuePicturesAsync(productAttributeValue.Id);
                    foreach (var oldValuePicture in oldValuePictures)
                    {
                        if (!originalNewPictureIdentifiers.TryGetValue(oldValuePicture.PictureId, out var valuePictureId))
                            continue;

                        await _productAttributeService.InsertProductAttributeValuePictureAsync(new ProductAttributeValuePicture
                        {
                            ProductAttributeValueId = attributeValueCopy.Id,
                            PictureId = valuePictureId
                        });
                    }

                    //picture associated to "iamge square" attribute type (if exists)
                    if (productAttributeValue.ImageSquaresPictureId > 0)
                    {
                        var origImageSquaresPicture =
                            await _pictureService.GetPictureByIdAsync(productAttributeValue.ImageSquaresPictureId);
                        if (origImageSquaresPicture != null)
                        {
                            //copy the picture
                            var imageSquaresPictureCopy = await _pictureService.InsertPictureAsync(
                                await _pictureService.LoadPictureBinaryAsync(origImageSquaresPicture),
                                origImageSquaresPicture.MimeType,
                                origImageSquaresPicture.SeoFilename,
                                origImageSquaresPicture.AltAttribute,
                                origImageSquaresPicture.TitleAttribute);
                            attributeValueCopy.ImageSquaresPictureId = imageSquaresPictureCopy.Id;
                        }
                    }

                    await _productAttributeService.InsertProductAttributeValueAsync(attributeValueCopy);

                   

                    int variantId = await _productExtendedService.GenerateVariantIdAsync(productAttributeMappingCopy, attributeValueCopy.Id, attributeValueCopy.Name);



                    if (variantId > 0)
                    {
                        attributeValueCopy.VariantId = variantId;
                        await _customProductAttributeService.UpdateProductAttributeValueWithoutEventAsync(attributeValueCopy);
                    }


                    //save associated value (used for combinations copying)
                    associatedAttributeValues.Add(productAttributeValue.Id, attributeValueCopy.Id);

                    //localization
                    foreach (var lang in languages)
                    {
                        var name = await _localizationService.GetLocalizedAsync(productAttributeValue, x => x.Name, lang.Id, false, false);
                        if (!string.IsNullOrEmpty(name))
                            await _localizedEntityService.SaveLocalizedValueAsync(attributeValueCopy, x => x.Name, name, lang.Id);
                    }
                }
            }

            //copy attribute conditions
            foreach (var productAttributeMapping in oldCopyWithConditionAttributes)
            {
                var oldConditionAttributeMapping = (await _productAttributeParser
                    .ParseProductAttributeMappingsAsync(productAttributeMapping.ConditionAttributeXml)).FirstOrDefault();

                if (oldConditionAttributeMapping == null)
                    continue;

                var oldConditionValues = await _productAttributeParser.ParseProductAttributeValuesAsync(
                    productAttributeMapping.ConditionAttributeXml,
                    oldConditionAttributeMapping.Id);

                if (!oldConditionValues.Any())
                    continue;

                var newAttributeMappingId = associatedAttributes[oldConditionAttributeMapping.Id];
                var newConditionAttributeMapping = productAttributeMappingCopies[newAttributeMappingId];

                var newConditionAttributeXml = string.Empty;

                foreach (var oldConditionValue in oldConditionValues)
                {
                    newConditionAttributeXml = _productAttributeParser.AddProductAttribute(newConditionAttributeXml,
                        newConditionAttributeMapping, associatedAttributeValues[oldConditionValue.Id].ToString());
                }

                var attributeMappingId = associatedAttributes[productAttributeMapping.Id];
                var conditionAttribute = productAttributeMappingCopies[attributeMappingId];
                conditionAttribute.ConditionAttributeXml = newConditionAttributeXml;

                await _productAttributeService.UpdateProductAttributeMappingAsync(conditionAttribute);
            }

            //attribute combinations
            foreach (var combination in await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id))
            {
                //generate new AttributesXml according to new value IDs
                var newAttributesXml = string.Empty;
                var parsedProductAttributes = await _productAttributeParser.ParseProductAttributeMappingsAsync(combination.AttributesXml);
                foreach (var oldAttribute in parsedProductAttributes)
                {
                    if (!associatedAttributes.ContainsKey(oldAttribute.Id))
                        continue;

                    var newAttribute = await _productAttributeService.GetProductAttributeMappingByIdAsync(associatedAttributes[oldAttribute.Id]);

                    if (newAttribute == null)
                        continue;

                    var oldAttributeValuesStr = _productAttributeParser.ParseValues(combination.AttributesXml, oldAttribute.Id);

                    foreach (var oldAttributeValueStr in oldAttributeValuesStr)
                    {
                        if (newAttribute.ShouldHaveValues())
                        {
                            //attribute values
                            var oldAttributeValue = int.Parse(oldAttributeValueStr);
                            if (!associatedAttributeValues.ContainsKey(oldAttributeValue))
                                continue;

                            var newAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(associatedAttributeValues[oldAttributeValue]);

                            if (newAttributeValue != null)
                            {
                                newAttributesXml = _productAttributeParser.AddProductAttribute(newAttributesXml,
                                    newAttribute, newAttributeValue.Id.ToString());
                            }
                        }
                        else
                        {
                            //just a text
                            newAttributesXml = _productAttributeParser.AddProductAttribute(newAttributesXml,
                                newAttribute, oldAttributeValueStr);
                        }
                    }
                }

                var combinationCopy = new ProductAttributeCombination
                {
                    ProductId = productCopy.Id,
                    AttributesXml = newAttributesXml,
                    StockQuantity = combination.StockQuantity,
                    MinStockQuantity = combination.MinStockQuantity,
                    AllowOutOfStockOrders = combination.AllowOutOfStockOrders,
                    Sku = combination.Sku,
                    ManufacturerPartNumber = combination.ManufacturerPartNumber,
                    Gtin = combination.Gtin,
                    OverriddenPrice = combination.OverriddenPrice,
                    NotifyAdminForQuantityBelow = combination.NotifyAdminForQuantityBelow,
                    OverriddenMsrp = combination.OverriddenMsrp,
                    OverriddenOldPrice = combination.OverriddenOldPrice,
                };
                await _productAttributeService.InsertProductAttributeCombinationAsync(combinationCopy);

                //picture
                var oldCombinationPictures = await _productAttributeService.GetProductAttributeCombinationPicturesAsync(combination.Id);
                foreach (var oldCombinationPicture in oldCombinationPictures)
                {
                    if (!originalNewPictureIdentifiers.TryGetValue(oldCombinationPicture.PictureId, out var combinationPictureId))
                        continue;

                    await _productAttributeService.InsertProductAttributeCombinationPictureAsync(new ProductAttributeCombinationPicture
                    {
                        ProductAttributeCombinationId = combinationCopy.Id,
                        PictureId = combinationPictureId
                    });
                }

                //quantity change history
                await _productService.AddStockQuantityHistoryEntryAsync(productCopy, combinationCopy.StockQuantity,
                    combinationCopy.StockQuantity,
                    message: string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.CopyProduct"), product.Id), combinationId: combinationCopy.Id);
            }
        }


        #endregion
    }
}
