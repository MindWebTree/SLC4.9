using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MWT.Nop.Core.Services.Catalog
{
    public partial class CustomProductAttributeParser : ProductAttributeParser, ICustomProductAttributeParser
    {
        public CustomProductAttributeParser(ICurrencyService currencyService, IDownloadService downloadService, ILocalizationService localizationService, IProductAttributeService productAttributeService, IRepository<ProductAttributeValue> productAttributeValueRepository, IWorkContext workContext) : base(currencyService, downloadService, localizationService, productAttributeService, productAttributeValueRepository, workContext)
        {
        }


        public virtual async Task<Dictionary<string, List<int>>> CustomGenerateAllCombinationsAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            List<ProductAttributeMapping> _allProductAttributeMappings = (await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id)).ToList();

            var productAttributes = await _productAttributeService.GetProductAttributeByIdsAsync(_allProductAttributeMappings.Select(a => a.ProductAttributeId).ToArray());
            var _settingService = EngineContext.Current.Resolve<ISettingService>();
            var stainAtttributeName = await _settingService.GetSettingByKeyAsync<string>("catalog.product.attribute.shade.name");

            _allProductAttributeMappings = (from _mapping in _allProductAttributeMappings
                                            join _productAttribute in productAttributes
                                            on _mapping.ProductAttributeId equals _productAttribute.Id
                                            where _productAttribute.Name != stainAtttributeName
                                            orderby _mapping.ProductAttributeId
                                            select _mapping).ToList();

            List<ProductAttributeMapping> allProductAttributeMappings = new List<ProductAttributeMapping>();
            foreach (var mapping in _allProductAttributeMappings)
            {
                if ((await _productAttributeService.GetProductAttributeValuesAsync(mapping.Id)).Where(pv => pv.Published == true).Any())
                {
                    allProductAttributeMappings.Add(mapping);
                }
            }

            allProductAttributeMappings = allProductAttributeMappings.Where(x => !x.IsNonCombinable()).ToList();





            //get all possible attribute combinations
            var allPossibleAttributeCombinations = CustomCreateCombination(allProductAttributeMappings);

            var allAttributesXml = new List<string>();



            foreach (var combination in allPossibleAttributeCombinations)
            {
                int counter = 1;
                var attributesXml = new List<string>();
                foreach (var productAttributeMapping in combination)
                {
                    if (!productAttributeMapping.ShouldHaveValues())
                        continue;

                    //get product attribute values
                    var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(productAttributeMapping.Id);



                    if (!attributeValues.Any())
                        continue;

                    var isCheckbox = productAttributeMapping.AttributeControlType == AttributeControlType.Checkboxes ||
                                     productAttributeMapping.AttributeControlType ==
                                     AttributeControlType.ReadonlyCheckboxes;

                    var currentAttributesXml = new List<string>();

                    if (isCheckbox)
                    {
                        //add several values attribute types (checkboxes)

                        //checkboxes could have several values ticked
                        foreach (var oldXml in attributesXml.Any() ? attributesXml : new List<string> { string.Empty })
                        {
                            foreach (var checkboxCombination in CreateCombination(attributeValues))
                            {
                                var newXml = oldXml;
                                foreach (var checkboxValue in checkboxCombination)
                                    newXml = AddProductAttribute(newXml, productAttributeMapping, checkboxValue.Id.ToString());

                                if (!string.IsNullOrEmpty(newXml))
                                    currentAttributesXml.Add(newXml);
                            }
                        }
                    }
                    else
                    {
                        //add one value attribute types (dropdownlist, radiobutton, color squares)

                        foreach (var oldXml in attributesXml.Any() ? attributesXml : new List<string> { string.Empty })
                        {
                            currentAttributesXml.AddRange(attributeValues.Select(attributeValue =>
                                CustomAddProductAttribute(oldXml, productAttributeMapping, attributeValue.Id.ToString())));
                        }
                    }



                    attributesXml.Clear();
                    attributesXml.AddRange(currentAttributesXml);
                    counter++;
                }

                allAttributesXml.AddRange(attributesXml);
            }

            //validate conditional attributes (if specified)
            //minor workaround:
            //once it's done (validation), then we could have some duplicated combinations in result
            //we don't remove them here (for performance optimization) because anyway it'll be done in the "GenerateAllAttributeCombinations" method of ProductController
            for (var i = 0; i < allAttributesXml.Count; i++)
            {
                var attributesXml = allAttributesXml[i];
                foreach (var attribute in allProductAttributeMappings)
                {
                    var conditionMet = await IsConditionMetAsync(attribute, attributesXml);
                    if (conditionMet.HasValue && !conditionMet.Value)
                        allAttributesXml[i] = RemoveProductAttribute(attributesXml, attribute);
                }
            }

            Dictionary<string, List<int>> keys = new Dictionary<string, List<int>>();

            foreach (var xml in allAttributesXml)
            {
                var valuelist = await ParseProductAttributeValuesAsync(xml);

                keys.Add(xml, valuelist.Select(x => x.Id).ToList());
            }




            return keys;
        }

        public IList<Tuple<string, string>> CustomParseValuesWithQuantity(string attributesXml, int productAttributeMappingId)
        {
            var selectedValues = new List<Tuple<string, string>>();
            if (string.IsNullOrEmpty(attributesXml))
                return selectedValues;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                foreach (XmlNode attributeNode in xmlDoc.SelectNodes(@"//Attributes/ProductAttribute"))
                {
                    if (attributeNode.Attributes?["ID"] == null)
                        continue;

                    if (!int.TryParse(attributeNode.Attributes["ID"].InnerText.Trim(), out var attributeId) ||
                        attributeId != productAttributeMappingId)
                        continue;

                    foreach (XmlNode attributeValue in attributeNode.SelectNodes("ProductAttributeValue"))
                    {
                        var value = attributeValue.SelectSingleNode("Value").InnerText.Trim();
                        var quantityNode = attributeValue.SelectSingleNode("Quantity");
                        selectedValues.Add(new Tuple<string, string>(value, quantityNode != null ? quantityNode.InnerText.Trim() : string.Empty));
                    }
                }
            }
            catch
            {
                // ignored
            }

            return selectedValues;
        }

        #region Utilties

        protected virtual IList<IList<T>> CustomCreateCombination<T>(IList<T> elements)
        {
            var rez = new List<IList<T>>();

            for (var i = 1; i < Math.Pow(2, elements.Count); i++)
            {
                var current = new List<T>();
                var index = -1;

                //transform int to binary string
                var binaryMask = Convert.ToString(i, 2).PadLeft(elements.Count, '0');

                foreach (var flag in binaryMask)
                {
                    index++;

                    if (flag == '0')
                        continue;

                    //add element if binary mask in the position of element has 1
                    current.Add(elements[index]);
                }
                if (current.Count == elements.Count)
                    rez.Add(current);
            }

            return rez;
        }
        protected virtual async Task<string> GetCustomProductAttributesXmlAsync(Product product, IFormCollection form, List<string> errors, string formId)
        {
            var attributesXml = string.Empty;
            var productAttributes = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            foreach (var attribute in productAttributes)
            {
                var controlId = $"{formId}{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}";
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var selectedAttributeId = int.Parse(ctrlAttributes);
                                if (selectedAttributeId > 0)
                                {
                                    //get quantity entered by customer
                                    var quantity = 1;
                                    var quantityStr = form[$"{formId}{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}_{selectedAttributeId}_qty"];
                                    if (!StringValues.IsNullOrEmpty(quantityStr) &&
                                        (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                                        errors.Add(await _localizationService.GetResourceAsync("Products.QuantityShouldBePositive"));

                                    attributesXml = AddProductAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString(), quantity > 1 ? (int?)quantity : null);
                                }
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                foreach (var item in ctrlAttributes.ToString()
                                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                    {
                                        //get quantity entered by customer
                                        var quantity = 1;
                                        var quantityStr = form[$"{formId}{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}_{item}_qty"];
                                        if (!StringValues.IsNullOrEmpty(quantityStr) &&
                                            (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                                            errors.Add(await _localizationService.GetResourceAsync("Products.QuantityShouldBePositive"));

                                        attributesXml = AddProductAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString(), quantity > 1 ? (int?)quantity : null);
                                    }
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                //get quantity entered by customer
                                var quantity = 1;
                                var quantityStr = form[$"{formId}{NopCatalogDefaults.ProductAttributePrefix}{attribute.Id}_{selectedAttributeId}_qty"];
                                if (!StringValues.IsNullOrEmpty(quantityStr) &&
                                    (!int.TryParse(quantityStr, out quantity) || quantity < 1))
                                    errors.Add(await _localizationService.GetResourceAsync("Products.QuantityShouldBePositive"));

                                attributesXml = AddProductAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString(), quantity > 1 ? (int?)quantity : null);
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = form[controlId];
                            if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                            {
                                var enteredText = ctrlAttributes.ToString().Trim();
                                attributesXml = AddProductAttribute(attributesXml, attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var day = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day));
                            }
                            catch
                            {
                                // ignored
                            }

                            if (selectedDate.HasValue)
                                attributesXml = AddProductAttribute(attributesXml, attribute, selectedDate.Value.ToString("D"));
                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            Guid.TryParse(form[controlId], out var downloadGuid);
                            var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                            if (download != null)
                                attributesXml = AddProductAttribute(attributesXml,
                                    attribute, download.DownloadGuid.ToString());
                        }
                        break;
                    default:
                        break;
                }
            }
            //validate conditional attributes (if specified)
            foreach (var attribute in productAttributes)
            {
                var conditionMet = await IsConditionMetAsync(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                {
                    attributesXml = RemoveProductAttribute(attributesXml, attribute);
                }
            }
            return attributesXml;
        }
        public virtual string CustomAddProductAttribute(string attributesXml, ProductAttributeMapping productAttributeMapping, string value, int? quantity = null)
        {
            var result = string.Empty;
            try
            {
                var xmlDoc = new XmlDocument();
                if (string.IsNullOrEmpty(attributesXml))
                {
                    var element1 = xmlDoc.CreateElement("Attributes");
                    xmlDoc.AppendChild(element1);
                }
                else
                {
                    xmlDoc.LoadXml(attributesXml);
                }

                var rootElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes");

                XmlElement attributeElement = null;
                //find existing
                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/ProductAttribute");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes?["ID"] == null)
                        continue;

                    var str1 = node1.Attributes["ID"].InnerText.Trim();
                    if (!int.TryParse(str1, out var id))
                        continue;

                    if (id != productAttributeMapping.Id)
                        continue;

                    attributeElement = (XmlElement)node1;
                    break;
                }

                //create new one if not found
                if (attributeElement == null)
                {
                    attributeElement = xmlDoc.CreateElement("ProductAttribute");
                    attributeElement.SetAttribute("ID", productAttributeMapping.Id.ToString());
                    rootElement.AppendChild(attributeElement);
                }

                var attributeValueElement = xmlDoc.CreateElement("ProductAttributeValue");
                attributeElement.AppendChild(attributeValueElement);

                var attributeValueValueElement = xmlDoc.CreateElement("Value");
                attributeValueValueElement.InnerText = value;
                attributeValueElement.AppendChild(attributeValueValueElement);

                //the quantity entered by the customer
                if (quantity.HasValue)
                {
                    var attributeValueQuantity = xmlDoc.CreateElement("Quantity");
                    attributeValueQuantity.InnerText = quantity.ToString();
                    attributeValueElement.AppendChild(attributeValueQuantity);
                }

                result = xmlDoc.OuterXml;
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            return result;
        }


        #endregion
    }
}
