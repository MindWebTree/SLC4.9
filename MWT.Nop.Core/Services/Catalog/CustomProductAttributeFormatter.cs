using MWT.Nop.Core.Infrastructure;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Tax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial class CustomProductAttributeFormatter : ProductAttributeFormatter, ICustomProductAttributeFormatter
    {
 
        public CustomProductAttributeFormatter(ICurrencyService currencyService, IDownloadService downloadService, IHtmlFormatter htmlFormatter, ILocalizationService localizationService, IPriceCalculationService priceCalculationService, IPriceFormatter priceFormatter, IProductAttributeParser productAttributeParser, IProductAttributeService productAttributeService, ITaxService taxService, IWebHelper webHelper, IWorkContext workContext, IStoreContext storeContext, ShoppingCartSettings shoppingCartSettings) : base(currencyService, downloadService, htmlFormatter, localizationService, priceCalculationService, priceFormatter, productAttributeParser, productAttributeService, taxService, webHelper, workContext, storeContext, shoppingCartSettings)
        {
        }
        public virtual async Task<string> CustomFormatAttributesAsync(Product product, string attributesXml)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            return await CustomFormatAttributesAsync(product, attributesXml, customer);
        }

        public virtual async Task<string> CustomFormatAttributesAsync(Product product, string attributesXml,
    Customer customer, string separator = "<br />", bool htmlEncode = true, bool renderPrices = true,
    bool renderProductAttributes = true, bool renderGiftCardAttributes = true,
    bool allowHyperlinks = true)
        {
            var result = new StringBuilder();
            var _settingService = EngineContext.Current.Resolve<ISettingService>();
            var attrShadeName = await _settingService.GetSettingByKeyAsync<string>("catalog.product.attribute.shade.name");
            var attrsMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            //attributes
            if (renderProductAttributes)
            {
                foreach (var attribute in await _productAttributeParser.ParseProductAttributeMappingsAsync(attributesXml))
                {
                    var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId);
                    var attributeName = await _localizationService.GetLocalizedAsync(productAttribute, a => a.Name, (await _workContext.GetWorkingLanguageAsync()).Id);
                    var textPrompt = attrsMapping.Where(attrsMapping => attrsMapping.ProductAttributeId == (productAttribute?.Id ?? 0)).FirstOrDefault()?.TextPrompt ?? string.Empty;
                    textPrompt = string.IsNullOrEmpty(textPrompt) ? attributeName : textPrompt;
                    var store = await _storeContext.GetCurrentStoreAsync();
                    //attributes without values
                    if (!attribute.ShouldHaveValues())
                    {
                        foreach (var value in _productAttributeParser.ParseValues(attributesXml, attribute.Id))
                        {
                            var formattedAttribute = string.Empty;
                            if (attribute.AttributeControlType == AttributeControlType.MultilineTextbox)
                            {
                                //encode (if required)
                                if (htmlEncode)
                                    attributeName = WebUtility.HtmlEncode(attributeName);

                                //we never encode multiline textbox input

                                if (attrShadeName == attributeName)
                                {
                                    formattedAttribute = $"{WebUtility.HtmlEncode(textPrompt)}: {_htmlFormatter.FormatText(CustomCommonHelper.StripUnwantedPrefixFromShade(value), false, true, false, false, false, false)}";
                                }
                                else
                                {
                                    formattedAttribute = $"{WebUtility.HtmlEncode(textPrompt)}: {_htmlFormatter.FormatText(value, false, true, false, false, false, false)}";
                                }
                            }
                            else if (attribute.AttributeControlType == AttributeControlType.FileUpload)
                            {
                                //file upload
                                Guid.TryParse(value, out var downloadGuid);
                                var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                                if (download != null)
                                {
                                    var fileName = $"{download.Filename ?? download.DownloadGuid.ToString()}{download.Extension}";

                                    //encode (if required)
                                    if (htmlEncode)
                                        fileName = WebUtility.HtmlEncode(fileName);

                                    var attributeText = allowHyperlinks ? $"<a href=\"{_webHelper.GetStoreLocation()}download/getfileupload/?downloadId={download.DownloadGuid}\" class=\"fileuploadattribute\">{fileName}</a>"
                                        : fileName;

                                    //encode (if required)
                                    if (htmlEncode)
                                        attributeName = WebUtility.HtmlEncode(attributeName);

                                    formattedAttribute = $"{WebUtility.HtmlEncode(textPrompt)}: {attributeText}";
                                }
                            }
                            else
                            {
                                //other attributes (textbox, datepicker)
                                formattedAttribute = $"{WebUtility.HtmlEncode(textPrompt)}: {value}";

                                //encode (if required)
                                if (htmlEncode)
                                    formattedAttribute = WebUtility.HtmlEncode(formattedAttribute);
                            }

                            if (string.IsNullOrEmpty(formattedAttribute))
                                continue;

                            if (result.Length > 0)
                                result.Append(separator);
                            result.Append(formattedAttribute);
                        }
                    }
                    //product attribute values
                    else
                    {
                        foreach (var attributeValue in await _productAttributeParser.ParseProductAttributeValuesAsync(attributesXml, attribute.Id))
                        {
                            var formattedAttribute = string.Empty;
                            if (attributeName == attrShadeName)
                            {
                                formattedAttribute = $"{WebUtility.HtmlEncode(textPrompt)}: {CustomCommonHelper.StripUnwantedPrefixFromShade(await _localizationService.GetLocalizedAsync(attributeValue, a => a.Name, (await _workContext.GetWorkingLanguageAsync()).Id))}";
                            }
                            else
                            {
                                formattedAttribute = $"{WebUtility.HtmlEncode(textPrompt)}: {await _localizationService.GetLocalizedAsync(attributeValue, a => a.Name, (await _workContext.GetWorkingLanguageAsync()).Id)}";
                            }
                            if (renderPrices)
                            {
                                if (attributeValue.PriceAdjustmentUsePercentage)
                                {
                                    if (attributeValue.PriceAdjustment > decimal.Zero)
                                    {
                                        formattedAttribute += string.Format(
                                                await _localizationService.GetResourceAsync("FormattedAttributes.PriceAdjustment"),
                                                "+", attributeValue.PriceAdjustment.ToString("G29"), "%");
                                    }
                                    else if (attributeValue.PriceAdjustment < decimal.Zero)
                                    {
                                        formattedAttribute += string.Format(
                                                await _localizationService.GetResourceAsync("FormattedAttributes.PriceAdjustment"),
                                                string.Empty, attributeValue.PriceAdjustment.ToString("G29"), "%");
                                    }
                                }
                                else
                                {
                                    var attributeValuePriceAdjustment = await _priceCalculationService.GetProductAttributeValuePriceAdjustmentAsync(product, attributeValue, customer,store);
                                    var (priceAdjustmentBase, _) = await _taxService.GetProductPriceAsync(product, attributeValuePriceAdjustment, customer);
                                    var priceAdjustment = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(priceAdjustmentBase, await _workContext.GetWorkingCurrencyAsync());

                                    if (priceAdjustmentBase > decimal.Zero)
                                    {
                                        formattedAttribute += string.Format(
                                                await _localizationService.GetResourceAsync("FormattedAttributes.PriceAdjustment"),
                                                "+", await _priceFormatter.FormatPriceAsync(priceAdjustment, false, false), string.Empty);
                                    }
                                    else if (priceAdjustmentBase < decimal.Zero)
                                    {
                                        formattedAttribute += string.Format(
                                                await _localizationService.GetResourceAsync("FormattedAttributes.PriceAdjustment"),
                                                "-", await _priceFormatter.FormatPriceAsync(-priceAdjustment, false, false), string.Empty);
                                    }
                                }
                            }

                            //display quantity
                            if (_shoppingCartSettings.RenderAssociatedAttributeValueQuantity && attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct)
                            {
                                //render only when more than 1
                                if (attributeValue.Quantity > 1)
                                    formattedAttribute += string.Format(await _localizationService.GetResourceAsync("ProductAttributes.Quantity"), attributeValue.Quantity);
                            }

                            //encode (if required)
                            if (htmlEncode)
                                formattedAttribute = WebUtility.HtmlEncode(formattedAttribute);

                            if (string.IsNullOrEmpty(formattedAttribute))
                                continue;

                            if (result.Length > 0)
                                result.Append(separator);
                            result.Append(formattedAttribute);
                        }
                    }
                }
            }

            //gift cards
            if (!renderGiftCardAttributes)
                return result.ToString();

            if (!product.IsGiftCard)
                return result.ToString();

            _productAttributeParser.GetGiftCardAttribute(attributesXml, out var giftCardRecipientName, out var giftCardRecipientEmail, out var giftCardSenderName, out var giftCardSenderEmail, out var _);

            //sender
            var giftCardFrom = product.GiftCardType == GiftCardType.Virtual ?
                string.Format(await _localizationService.GetResourceAsync("GiftCardAttribute.From.Virtual"), giftCardSenderName, giftCardSenderEmail) :
                string.Format(await _localizationService.GetResourceAsync("GiftCardAttribute.From.Physical"), giftCardSenderName);
            //recipient
            var giftCardFor = product.GiftCardType == GiftCardType.Virtual ?
                string.Format(await _localizationService.GetResourceAsync("GiftCardAttribute.For.Virtual"), giftCardRecipientName, giftCardRecipientEmail) :
                string.Format(await _localizationService.GetResourceAsync("GiftCardAttribute.For.Physical"), giftCardRecipientName);

            //encode (if required)
            if (htmlEncode)
            {
                giftCardFrom = WebUtility.HtmlEncode(giftCardFrom);
                giftCardFor = WebUtility.HtmlEncode(giftCardFor);
            }

            if (!string.IsNullOrEmpty(result.ToString()))
            {
                result.Append(separator);
            }

            result.Append(giftCardFrom);
            result.Append(separator);
            result.Append(giftCardFor);

            return result.ToString();
        }
    }
}
