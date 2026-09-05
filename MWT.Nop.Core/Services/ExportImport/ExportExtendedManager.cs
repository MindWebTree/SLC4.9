using MWT.Nop.Core.Domain.Catalog;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Infrastructure;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.ExportImport.Help;
using Nop.Services.Forums;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using System.Text;

namespace MWT.Nop.Core.Services.ExportImport
{
    
    public partial class ExportExtendedManager : ExportManager, IExportExtendedManager
    {
        #region Fields
        private readonly INopFileProvider _fileProvider;

        #endregion
        public ExportExtendedManager(AddressSettings addressSettings, CatalogSettings catalogSettings, SecuritySettings securitySettings, CustomerSettings customerSettings, DateTimeSettings dateTimeSettings, 
            ForumSettings forumSettings, IAddressService addressService, IAttributeFormatter<CustomerAttribute, CustomerAttributeValue> customerAttributeFormatter, ICategoryService categoryService, 
            ICountryService countryService, ICurrencyService currencyService, ICustomerActivityService customerActivityService, ICustomerService customerService, IDateRangeService dateRangeService, 
            IDateTimeHelper dateTimeHelper, IDiscountService discountService, IForumService forumService, IGdprService gdprService, IGenericAttributeService genericAttributeService, 
            ILanguageService languageService, ILocalizationService localizationService, ILocalizedEntityService localizedEntityService, IManufacturerService manufacturerService, IMeasureService measureService, 
            IOrderService orderService, IPictureService pictureService, IPriceFormatter priceFormatter, IProductAttributeService productAttributeService, IProductService productService, IProductTagService productTagService
            , IProductTemplateService productTemplateService, IShipmentService shipmentService, ISpecificationAttributeService specificationAttributeService, IStateProvinceService stateProvinceService, 
            IStoreMappingService storeMappingService, IStoreService storeService, ITaxCategoryService taxCategoryService, IThumbService thumbService, IUrlRecordService urlRecordService, 
            IVendorService vendorService, IWorkContext workContext, OrderSettings orderSettings, ProductEditorSettings productEditorSettings, INopFileProvider fileProvider) : base(addressSettings, catalogSettings, securitySettings, customerSettings, dateTimeSettings, forumSettings, addressService, customerAttributeFormatter, categoryService, countryService, currencyService, customerActivityService, customerService, dateRangeService, dateTimeHelper, discountService, forumService, gdprService, genericAttributeService, languageService, localizationService, localizedEntityService, manufacturerService, measureService, orderService, pictureService, priceFormatter, productAttributeService, productService, productTagService, productTemplateService, shipmentService, specificationAttributeService, stateProvinceService, storeMappingService, storeService, taxCategoryService, thumbService, urlRecordService, vendorService, workContext, orderSettings, productEditorSettings)
        {
            _fileProvider = fileProvider;
        }

        public virtual async Task<byte[]> ExportCategoryProductsToXlsxAsync(IList<ProductCategory> products)
        {
            //a vendor should have access only to part of order information
            var ignore = await _workContext.GetCurrentVendorAsync() != null;

            //lambda expressions for choosing correct order address


            //property array
            var manager = new PropertyManager<ProductCategory>(new[]
           {
                new PropertyByName<ProductCategory>("CategoryID", (p,_) => p.CategoryId),
                new PropertyByName<ProductCategory>("ProductID", (p, _) => p.ProductId),
                 new PropertyByName<ProductCategory>("DisplayOrder", (p, _) => p.DisplayOrder),
                 new PropertyByName<ProductCategory>("MobileDisplayOrder", (p, _) => p.MobileDisplayOrder)
                }, _catalogSettings);

            return await manager.ExportToXlsxAsync(products);
        }
        public virtual async Task<byte[]> ExportSpecificationAttributeOptionProductsToXlsxAsync(IList<ExportProductSpecificationAttributeFormat> products)
        {
            //a vendor should have access only to part of order information
            var ignore = await _workContext.GetCurrentVendorAsync() != null;

            //lambda expressions for choosing correct order address


            //property array
            var manager = new PropertyManager<ExportProductSpecificationAttributeFormat>(new[]
           {
                new PropertyByName<ExportProductSpecificationAttributeFormat>("SpecificationAttributeOptionId", (p, _) => p.SpecificationAttributeOptionId),
                new PropertyByName<ExportProductSpecificationAttributeFormat>("ProductID", (p, _) => p.ProductId),
                 new PropertyByName<ExportProductSpecificationAttributeFormat>("DisplayOrder", (p, _) => p.DisplayOrder),
                new PropertyByName<ExportProductSpecificationAttributeFormat>("MobileDisplayOrder", (p, _) => p.MobileDisplayOrder),
                  new PropertyByName<ExportProductSpecificationAttributeFormat>("CategoryIds", (p, _) => p.CategoryIds),
                }, _catalogSettings);

            return await manager.ExportToXlsxAsync(products);
        }

        public virtual async Task ExportProductsToCsvAsync(List<object> records, string folder, string fileName)
        {
            var sb = new StringBuilder();
            List<string> columnNames = new List<string>();



            foreach (object rec in records)
            {
                if (columnNames.Count == 0)
                {
                    columnNames = new List<string>(rec.GetType().GetProperties().Select(i => i.Name));
                    foreach (var column in columnNames)
                    {
                        if (column != "CustomProperties")
                        {
                            sb.Append(column + ",");
                        }
                    }
                    sb.AppendLine();
                }
                foreach (string prop in columnNames)
                {

                    try
                    {
                        if (prop != "CustomProperties")
                        {
                            sb.Append($"{rec.GetType().GetProperty(prop).GetValue(rec, null)},");
                        }
                    }
                    catch (Exception exp)
                    {

                    }
                }
                sb.AppendLine();
            }


            if (!_fileProvider.DirectoryExists(_fileProvider.MapPath($"/wwwroot/{folder}")))
            {
                _fileProvider.CreateDirectory(_fileProvider.MapPath($"/wwwroot/{folder}"));
            }
            if (_fileProvider.FileExists(_fileProvider.MapPath($"/wwwroot/{folder}/{fileName}")))
            {
                _fileProvider.DeleteFile(_fileProvider.MapPath($"/wwwroot/{folder}/{fileName}"));
            }
            var filePath = _fileProvider.MapPath($"/wwwroot/{folder}/{fileName}");
            _fileProvider.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

        }

        public virtual async Task<byte[]> ExportProductAttributeCombinationToXlsxAsync(IList<ExportAttrCombination> attributeCombinations)
        {
            //a vendor should have access only to part of order information
            var ignore = await _workContext.GetCurrentVendorAsync() != null;

            //lambda expressions for choosing correct order address


            //property array
            var manager = new PropertyManager<ExportAttrCombination>(new[]
           {
                new PropertyByName<ExportAttrCombination>("ProductId", (p, _) => p.ProductId),
                new PropertyByName<ExportAttrCombination>("AttributeXml", (p, _) => p.AttributeXml),
                new PropertyByName<ExportAttrCombination>("AttributeDescription", (p, _) => p.AttributeDescription),
                new PropertyByName<ExportAttrCombination>("Msrp", (p, _) => p.Msrp),
                new PropertyByName<ExportAttrCombination>("Price", (p, _) => p.Price),
                new PropertyByName<ExportAttrCombination>("SalePrice", (p, _) => p.SalePrice),
        }, _catalogSettings);

            return await manager.ExportToXlsxAsync(attributeCombinations);
        }
    }

}
