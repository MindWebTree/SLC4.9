using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Services.Catalog;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.FilterLevels;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Http;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.ExportImport;
using Nop.Services.ExportImport.Help;
using Nop.Services.FilterLevels;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;

namespace MWT.Nop.Core.Services.ExportImport
{
    public partial class ImportExtendedManager : ImportManager, IImportExtendedManager
    {
        #region Fields

        private readonly ICustomSpecificationAttributeService _customSpecificationAttributeService;
        private readonly IProductExtendedService _productExtendedService;

        #endregion
        public ImportExtendedManager(CatalogSettings catalogSettings, IAddressService addressService, IBackInStockSubscriptionService backInStockSubscriptionService, ICategoryService categoryService,
            ICountryService countryService, ICustomerActivityService customerActivityService, ICustomerService customerService, ICustomNumberFormatter customNumberFormatter, INopDataProvider dataProvider,
            IDateRangeService dateRangeService, IFilterLevelValueService filterLevelValueService, IGenericAttributeService genericAttributeService, IHttpClientFactory httpClientFactory, ILanguageService languageService
            , ILocalizationService localizationService, ILocalizedEntityService localizedEntityService, ILogger logger, IManufacturerService manufacturerService, IMeasureService measureService,
            INewsLetterSubscriptionService newsLetterSubscriptionService, INewsLetterSubscriptionTypeService newsLetterSubscriptionTypeService, INopFileProvider fileProvider, IOrderService orderService,
            IPictureService pictureService, IProductAttributeService productAttributeService, IProductService productService, IProductTagService productTagService, IProductTemplateService productTemplateService,
            IServiceScopeFactory serviceScopeFactory, ISpecificationAttributeService specificationAttributeService, IStateProvinceService stateProvinceService, IStoreContext storeContext,
            IStoreMappingService storeMappingService, IStoreService storeService, ITaxCategoryService taxCategoryService, IUrlRecordService urlRecordService, IVendorService vendorService,
            IWarehouseService warehouseService, IWorkContext workContext, MediaSettings mediaSettings, SecuritySettings securitySettings, TaxSettings taxSettings,
            VendorSettings vendorSettings, ICustomSpecificationAttributeService customSpecificationAttributeService, IProductExtendedService productExtendedService) : base(catalogSettings, addressService, backInStockSubscriptionService, categoryService, countryService, customerActivityService, customerService, customNumberFormatter, dataProvider, dateRangeService, filterLevelValueService, genericAttributeService, httpClientFactory, languageService, localizationService, localizedEntityService, logger, manufacturerService, measureService, newsLetterSubscriptionService, newsLetterSubscriptionTypeService, fileProvider, orderService, pictureService, productAttributeService, productService, productTagService, productTemplateService, serviceScopeFactory, specificationAttributeService, stateProvinceService, storeContext, storeMappingService, storeService, taxCategoryService, urlRecordService, vendorService, warehouseService, workContext, mediaSettings, securitySettings, taxSettings, vendorSettings)
        {
            _customSpecificationAttributeService = customSpecificationAttributeService;
            _productExtendedService = productExtendedService;
        }

        public virtual async Task ImportCategoryProductsFromXlsxAsync(Stream stream, int categoryid)
        {

            using var workbook = new XLWorkbook(stream);

            var languages = await _languageService.GetAllLanguagesAsync(showHidden: true);

            //the columns
            var metadata = GetWorkbookMetadata<ProductCategory>(workbook, languages);
            var defaultWorksheet = metadata.DefaultWorksheet;
            var defaultProperties = metadata.DefaultProperties;

            var manager = new PropertyManager<ProductCategory>(defaultProperties, _catalogSettings);

            var iRow = 2;

            var products = await _categoryService.GetProductCategoriesByCategoryIdAsync(categoryid, 0, int.MaxValue);
            while (true)
            {
                var allColumnsAreEmpty = manager.GetDefaultProperties
                .Select(property => defaultWorksheet.Row(iRow).Cell(property.PropertyOrderPosition))
                .All(cell => cell?.Value == null || string.IsNullOrEmpty(cell.Value.ToString()));

                if (allColumnsAreEmpty)
                    break;

                manager.ReadDefaultFromXlsx(defaultWorksheet, iRow);


                int productId = manager.GetDefaultProperty("ProductID").IntValue;
                int displayOrder = manager.GetDefaultProperty("DisplayOrder").IntValue;
                int mobileDisplayOrder = manager.GetDefaultProperty("MobileDisplayOrder").IntValue;
                var prdcategory = products.Where(p => p.CategoryId == categoryid && p.ProductId == productId).FirstOrDefault();

                if (prdcategory != null)
                {
                    bool needUpdate = false;

                    if (prdcategory.DisplayOrder != displayOrder)
                    {
                        needUpdate = true;
                        prdcategory.DisplayOrder = displayOrder;
                    }
                    if (prdcategory.MobileDisplayOrder != mobileDisplayOrder)
                    {
                        needUpdate = true;
                        prdcategory.MobileDisplayOrder = mobileDisplayOrder;
                    }
                    if (needUpdate)
                    {
                        await _categoryService.UpdateProductCategoryAsync(prdcategory);
                    }
                }
                else
                {
                    var product = await _productService.GetProductByIdAsync(productId);
                    if (product != null)
                    {
                        ProductCategory prdCategory = new ProductCategory();
                        prdCategory.CategoryId = categoryid;
                        prdCategory.ProductId = productId;
                        prdCategory.DisplayOrder = displayOrder;
                        prdCategory.MobileDisplayOrder = mobileDisplayOrder;
                        await _categoryService.InsertProductCategoryAsync(prdCategory);
                    }
                }
                iRow++;

            }
        }

        public virtual async Task ImportSpecificationAttributeOptionProductsFromXlsxAsync(Stream stream, int specificationAttributeOptionId)
        {
            using var workbook = new XLWorkbook(stream);

            var languages = await _languageService.GetAllLanguagesAsync(showHidden: true);

            //the columns
            var metadata = GetWorkbookMetadata<ProductSpecificationAttribute>(workbook, languages);
            var defaultWorksheet = metadata.DefaultWorksheet;
            var defaultProperties = metadata.DefaultProperties;

            var manager = new PropertyManager<ProductSpecificationAttribute>(defaultProperties, _catalogSettings);
            var iRow = 2;

            var products = await _customSpecificationAttributeService.GetProductsBySpecificationAttributeOptionIdAsync(specificationAttributeOptionId, 0, int.MaxValue);
            while (true)
            {
                var allColumnsAreEmpty = manager.GetDefaultProperties
         .Select(property => defaultWorksheet.Row(iRow).Cell(property.PropertyOrderPosition))
         .All(cell => cell?.Value == null || string.IsNullOrEmpty(cell.Value.ToString()));

                if (allColumnsAreEmpty)
                    break;

                manager.ReadDefaultFromXlsx(defaultWorksheet, iRow);
                int productId = manager.GetDefaultProperty("ProductID").IntValue;
                int displayOrder = manager.GetDefaultProperty("DisplayOrder").IntValue;
                int mobileDisplayOrder = manager.GetDefaultProperty("MobileDisplayOrder").IntValue;
                var prdSpecificationAttributeOption = products.Where(p => p.SpecificationAttributeOptionId == specificationAttributeOptionId && p.ProductId == productId).FirstOrDefault();
                if (prdSpecificationAttributeOption != null)
                {
                    bool needUpdate = false;
                    if (prdSpecificationAttributeOption.DisplayOrder != displayOrder)
                    {
                        needUpdate = true;
                        prdSpecificationAttributeOption.DisplayOrder = displayOrder;
                    }
                    if (prdSpecificationAttributeOption.MobileDisplayOrder != mobileDisplayOrder)
                    {
                        needUpdate = true;
                        prdSpecificationAttributeOption.MobileDisplayOrder = mobileDisplayOrder;
                    }
                    if (needUpdate)
                    {
                        await _specificationAttributeService.UpdateProductSpecificationAttributeAsync(prdSpecificationAttributeOption);
                    }
                }
                else
                {
                    var product = await _productService.GetProductByIdAsync(productId);
                    if (product != null)
                    {
                        prdSpecificationAttributeOption = new ProductSpecificationAttribute();
                        prdSpecificationAttributeOption.SpecificationAttributeOptionId = specificationAttributeOptionId;
                        prdSpecificationAttributeOption.ProductId = productId;
                        prdSpecificationAttributeOption.MobileDisplayOrder = mobileDisplayOrder;
                        prdSpecificationAttributeOption.DisplayOrder = displayOrder;
                        await _specificationAttributeService.InsertProductSpecificationAttributeAsync(prdSpecificationAttributeOption);
                    }
                }
                iRow++;

            }
        }


        public virtual async Task ImportProductAttributeCombinationFromXlsxAsync(Stream stream)
        {
            using var workbook = new XLWorkbook(stream);

            var languages = await _languageService.GetAllLanguagesAsync(showHidden: true);

            //the columns
            var metadata = GetWorkbookMetadata<ProductAttributeCombination>(workbook, languages);
            var defaultWorksheet = metadata.DefaultWorksheet;
            var defaultProperties = metadata.DefaultProperties;

            var manager = new PropertyManager<ProductAttributeCombination>(defaultProperties, _catalogSettings);
            var iRow = 2;
            manager.ReadDefaultFromXlsx(defaultWorksheet, iRow);
            var product = await _productService.GetProductByIdAsync(manager.GetDefaultProperty("ProductId").IntValue);
            var _productAttributeParser = EngineContext.Current.Resolve<IProductAttributeParser>();
            while (true && product != null)
            {
                var allColumnsAreEmpty = manager.GetDefaultProperties
        .Select(property => defaultWorksheet.Row(iRow).Cell(property.PropertyOrderPosition))
        .All(cell => cell?.Value == null || string.IsNullOrEmpty(cell.Value.ToString()));

                if (allColumnsAreEmpty)
                    break;
                manager.ReadDefaultFromXlsx(defaultWorksheet, iRow);
                var xmlMapping = manager.GetDefaultProperty("AttributeXml").StringValue;
                var msrp = manager.GetDefaultProperty("Msrp").DecimalValue;
                var price = manager.GetDefaultProperty("Price").DecimalValue;
                var salesPrice = manager.GetDefaultProperty("SalePrice").DecimalValue;
                ProductAttributeCombination combination = new ProductAttributeCombination();
                if (xmlMapping.Length > 0 && price > 0 && salesPrice > 0)
                {
                    combination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, xmlMapping);
                    if (combination != null)
                    {
                        combination.OverriddenMsrp = msrp;
                        combination.OverriddenOldPrice = price;
                        combination.OverriddenPrice = salesPrice;
                        await _productAttributeService.UpdateProductAttributeCombinationAsync(combination);
                    }
                }
                iRow++;
            }
            if (product != null)
            {
                await _productExtendedService.GetVariantPriceRange(product, true, true);
            }
        }


    }
}