using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Domain.Zoho;
using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Service.Catalog;
using MWT.Nop.Core.Service.Zoho;
using MWT.Nop.Core.Services.Catalog;
using MWT.Nop.Core.Services.MailChimp;
using MWT.Nop.Core.Services.Message;
using MWT.Nop.Core.Services.Zoho;
using MWT.Plugin.Misc.MwtStorefront.Factories;
using MWT.Plugin.Misc.MwtStorefront.Models.Catalog;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System.Text;

namespace MWT.Plugin.Misc.MwtStorefront.Controller
{

    [AutoValidateAntiforgeryToken]
    public partial class CustomizationFormController : BasePublicController
    {
        #region Fields
        private readonly ICustomProductService _productService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        protected readonly ICustomRecentlyViewedProductsService _recentlyViewedProductsService;
        protected readonly ICustomerActivityService _customerActivityService;
        protected readonly ILocalizationService _localizationService;
        private readonly ICustomProductModelFactory _productModelFactory;
        private readonly IWebHelper _webHelper;
        private readonly ICustomSpecificationAttributeService _specificationAttributeService;
        private readonly ICustomCategoryService _categoryService;
        private readonly ICustomizationFormSerivce _customizationFormSerivce;
        private readonly CaptchaSettings _captchaSettings;
        private readonly IZohoService _zohoService;
        private readonly ICustomWorkflowMessageService _workflowMessageService;

        #endregion
        public CustomizationFormController(ICustomProductService productService, CatalogSettings catalogSettings, IAclService aclService, IStoreMappingService storeMappingService,
            IPermissionService permissionService, IUrlRecordService urlRecordService, ShoppingCartSettings shoppingCartSettings,
            IShoppingCartService shoppingCartService, IWorkContext workContext, IStoreContext storeContext, ICustomRecentlyViewedProductsService recentlyViewedProductsService,
            ICustomerActivityService customerActivityService, ILocalizationService localizationService, ICustomProductModelFactory productModelFactory, IWebHelper webHelper,
            ICustomSpecificationAttributeService specificationAttributeService, ICustomCategoryService categoryService, ICustomizationFormSerivce customizationFormSerivce, CaptchaSettings captchaSettings,
            IZohoService zohoService, ICustomWorkflowMessageService workflowMessageService)
        {
            _productService = productService;
            _catalogSettings = catalogSettings;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _permissionService = permissionService;
            _urlRecordService = urlRecordService;
            _shoppingCartSettings = shoppingCartSettings;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
            _storeContext = storeContext;
            _recentlyViewedProductsService = recentlyViewedProductsService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _productModelFactory = productModelFactory;
            _webHelper = webHelper;
            _specificationAttributeService = specificationAttributeService;
            _categoryService = categoryService;
            _customizationFormSerivce = customizationFormSerivce;
            _captchaSettings = captchaSettings;
            _zohoService = zohoService;
            _workflowMessageService = workflowMessageService;
        }


        #region Methods

        public virtual async Task<IActionResult> CustomizationForm(int productId, decimal width)
        {

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted)
                return InvokeHttp404();
            int customizationFormTemplateId = product.CustomizationFormTemplateId;
            if (customizationFormTemplateId == 0)
            {

                var categoryId = await _specificationAttributeService.GetMainCategoryOfProduct(product.Id);
                if (categoryId != 0)
                {
                    customizationFormTemplateId = (await _categoryService.GetCategoryByIdAsync(categoryId))?.CustomizationFormTemplateId ?? 0;
                }
            }

            if (customizationFormTemplateId == 0)
            {
                var templates = await _customizationFormSerivce.GetAllProductCustomizationFormTemplatesAsync();
                customizationFormTemplateId = templates.Where(t => t.IsDefault).FirstOrDefault()?.Id ?? 0;
                if (customizationFormTemplateId == 0)
                {
                    customizationFormTemplateId = templates.FirstOrDefault()?.Id ?? 0;
                }
            }
            if (customizationFormTemplateId == 0)
            {
                return InvokeHttp404();
            }
            var customizationFormTemplate = await _customizationFormSerivce.GetProductCustomizationFormTemplateByIdAsync(customizationFormTemplateId);
            if (customizationFormTemplate == null)
            {
                return InvokeHttp404();
            }

            return Json(new
            {
                content = await RenderPartialViewToStringAsync(width > 992 ? customizationFormTemplate.ViewPath : customizationFormTemplate.MobileViewPath, await this._productModelFactory.PrepareCustomizationFormModelAsync(product))
            });

        }

        [HttpPost]
        [ValidateCaptcha]
        public virtual async Task<IActionResult> CustomizationForm(CustomizationFormModel model, IFormCollection form, bool captchaValid)
        {
            var _settingService = EngineContext.Current.Resolve<ISettingService>();
            string fakeCustomerEmails = await _settingService.GetSettingByKeyAsync<string>("Common.FakeCustomer.Emails");
            if ((_captchaSettings.Enabled && !captchaValid) || CustomCommonHelper.IsFakeCustomer(fakeCustomerEmails, model.Email ?? string.Empty))
            {
                return Json(new
                {
                    content = await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"),
                    Success = false
                });
            }
            StringBuilder sbZohoDescription = new StringBuilder();
            if (ModelState.IsValid)
            {
                if (form == null)
                    throw new ArgumentNullException(nameof(form));

                if (model.ProductId == 0)
                {
                    return Json(new
                    {
                        content = "ProductId not exist!!!",
                        Success = false
                    });
                }

                else
                {
                    // check captcha


                    // end
                    var _productAttributeService = EngineContext.Current.Resolve<IProductAttributeService>();



                    var product = await _productService.GetProductByIdAsync(model.ProductId);
                    if (product == null || product.Deleted || !@product.Published)
                        throw new Exception("ProductId not exist!!!");

                    // Attachment
                    Dictionary<string, bool> dicAttachments = new Dictionary<string, bool>();


                    if (form.Files.Count > 0)
                    {

                        int maxFilesAllowed = await _settingService.GetSettingByKeyAsync<int>("Customizationform.Max.Files.Allow");
                        if (form.Files.Count <= maxFilesAllowed)
                        {

                            int uploadMaxSize = await _settingService.GetSettingByKeyAsync<int>("Customizationform.UploadMaxSize");
                            foreach (var _attachment in form.Files)
                            {
                                if (_attachment.Length <= uploadMaxSize)
                                {
                                    var supportedUploadTypes = (await _settingService.GetSettingByKeyAsync<string>("Customizationform.Supported.UploadTypes")).ToLower();
                                    if (supportedUploadTypes.Split(',').Contains(Path.GetExtension(_attachment.FileName).ToLower()))
                                    {
                                        string fileName = Guid.NewGuid().ToString() + "-" + _attachment.FileName;
                                        INopFileProvider _nopFileProvider = EngineContext.Current.Resolve<INopFileProvider>();
                                        string filePath = Path.Combine(_nopFileProvider.MapPath("/wwwroot/images/customizationform/"),
                                          fileName);
                                        using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                                        {
                                            await _attachment.CopyToAsync(fileStream);
                                            dicAttachments.Add(_storeContext.GetCurrentStore().Url + "/images/customizationform/" + fileName, (await _settingService.GetSettingByKeyAsync<string>("Customizationform.Supported.UploadTypes.Image")).ToLower().Split(',').Contains(Path.GetExtension(_attachment.FileName).ToLower()));
                                        }
                                    }
                                }
                                else
                                    return Json(new
                                    {
                                        content = string.Format(
                                        await _localizationService.GetResourceAsync("Customizationform.Fields.Attachment.MaxUploadSize")
                                        , uploadMaxSize),
                                        Success = false
                                    });
                            }
                        }
                        else
                            return Json(new
                            {
                                content =
                                await _localizationService.GetResourceAsync("customizationform.Image.Validity.Error"),
                                Success = false
                            });
                    }


                    CustomizationFormDataModel responseModel = new CustomizationFormDataModel();
                    responseModel.FullName = model.FullName;
                    responseModel.Phone = model.Phone;
                    responseModel.ProductId = product.Id;
                    sbZohoDescription.Append("ProductID" + " : " + product.Id + "\n________________\n");
                    responseModel.ProductName = product.Name;
                    responseModel.SKU = product.Sku;
                    sbZohoDescription.Append("SKU" + " : " + product.Sku + "\n________________\n");
                    responseModel.ZipCode = model.ZipCode;
                    responseModel.Email = model.Email;
                    responseModel.Dimensions = model.Dimensions;

                    sbZohoDescription.Append("Dimensions" + "\n________________\n");
                    if (product.Length > 0)
                        sbZohoDescription.Append("Length" + " : " + Math.Round(product.Length, 2) + "\n________________\n");
                    if (product.Width > 0)
                        sbZohoDescription.Append("Width" + " : " + Math.Round(product.Width, 2) + "\n________________\n");
                    if (product.Height > 0)
                        sbZohoDescription.Append("Height" + " : " + Math.Round(product.Height, 2) + "\n________________\n");

                    responseModel.Message = model.Message;

                    responseModel.Length = product.Length;
                    responseModel.Width = product.Width;
                    responseModel.Height = product.Height;
                    responseModel.Attachments = dicAttachments;
                    string productName = product.Name;

                    // Custom props
                    string picture = "";
                    Dictionary<string, string> customProps = new Dictionary<string, string>();
                    bool isCustomAttrSelected = false;
                    foreach (var key in form.Keys)
                    {
                        // Specification Attributes

                        if (key.ToLower().StartsWith(("custom_" + NopCatalogDefaults.ProductAttributePrefix).ToLower()))
                        {
                            int.TryParse(key.ToLower().Replace(("custom_" + NopCatalogDefaults.ProductAttributePrefix).ToLower(), ""),
                                out int parentAttributeId);
                            if (parentAttributeId > 0)
                            {
                                var values = form[key];
                                if (!string.IsNullOrEmpty(values))
                                {
                                    int.TryParse(values, out int childAttributeId);
                                    if (childAttributeId > 0)
                                    {
                                        var parentProductAttributeMapping = await _productAttributeService.GetProductAttributeMappingByIdAsync(parentAttributeId);
                                        if (parentProductAttributeMapping != null && parentProductAttributeMapping.ProductAttributeId > 0)
                                        {
                                            var parentProductAttribute = await _productAttributeService.GetProductAttributeByIdAsync(parentProductAttributeMapping.ProductAttributeId);
                                            if (!string.IsNullOrEmpty(parentProductAttribute.Name) && !customProps.ContainsKey((parentProductAttribute.Name)))
                                            {
                                                var childProductAttribute = await _productAttributeService.GetProductAttributeValueByIdAsync(childAttributeId);
                                                if (!string.IsNullOrEmpty(childProductAttribute.Name))
                                                {
                                                    var selectedAttrName = childProductAttribute.Name;


                                                    customProps.Add(await _localizationService.GetResourceAsync("customizationform." + parentProductAttribute.Name),
                                                      selectedAttrName);
                                                    productName += "-" + selectedAttrName;
                                                    sbZohoDescription.Append(await _localizationService.GetResourceAsync("customizationform." + parentProductAttribute.Name)
                                                        + " : " + selectedAttrName + "\n________________\n");
                                                }

                                            }
                                        }
                                    }
                                }


                                // end
                            }

                        }
                        else if (key.ToLower() == "intereseditems")
                        {
                            var values = form[key];
                            if (!string.IsNullOrEmpty(values))
                            {
                                customProps.Add(await _localizationService.GetResourceAsync("customizationform." + key), values);
                                sbZohoDescription.Append(await _localizationService.GetResourceAsync("customizationform." + key)
                                                    + " : " + values + "\n________________\n");
                            }
                        }
                        else if (key.ToLower() == "defaultpicture")
                        {
                            var values = form[key];
                            if (!string.IsNullOrEmpty(values))
                                picture = values;
                        }
                        else if (key.ToLower() == "custom_design")
                        {
                            customProps.Add(await _localizationService.GetResourceAsync("customizationform." + key), "True");
                            sbZohoDescription.Append(await _localizationService.GetResourceAsync("customizationform." + key)
                                                + " : " + "True" + "\n________________\n");
                        }
                        else if (key.ToLower() == "custom_design")
                        {
                            customProps.Add(await _localizationService.GetResourceAsync("customizationform." + key), "True");
                            sbZohoDescription.Append(await _localizationService.GetResourceAsync("customizationform." + key)
                                                + " : " + "True" + "\n________________\n");
                        }
                        else if (key.ToLower() == "customshade")
                        {
                            customProps.Add(await _localizationService.GetResourceAsync("customizationform." + key), await _localizationService.GetResourceAsync($"customizationform.{key}.Custom"));
                            sbZohoDescription.Append(await _localizationService.GetResourceAsync("customizationform." + key)
                                                + " : " + await _localizationService.GetResourceAsync($"customizationform.{key}.Custom") + "\n________________\n");
                        }
                        else if (key.ToLower() == "customsize")
                        {
                            if (!string.IsNullOrEmpty(form["customsizelength"].ToString()) || !string.IsNullOrEmpty(form["customsizewidth"].ToString()) || !string.IsNullOrEmpty(form["customsizeheight"].ToString()))
                            {
                                customProps.Add(await _localizationService.GetResourceAsync("customizationform." + key), $"{form["customsizelength"].ToString() ?? "0"}\" L  X {form["customsizewidth"].ToString() ?? "0"}\" D  X {form["customsizeheight"].ToString() ?? "0"}\" H");
                                sbZohoDescription.Append(await _localizationService.GetResourceAsync("customizationform." + key)
                                                    + " : " + $"{form["customsizelength"].ToString() ?? "0"}\" L  X {form["customsizewidth"].ToString() ?? "0"}\" D  X {form["customsizeheight"].ToString() ?? "0"}\" H" + "\n________________\n");
                            }
                            else
                            {
                                customProps.Add(await _localizationService.GetResourceAsync("customizationform." + key), $"{form["rugSize"].ToString() ?? "0"}");
                                sbZohoDescription.Append(await _localizationService.GetResourceAsync("customizationform." + key)
                                                    + " : " + $"{form["rugSize"].ToString() ?? "0"}" + "\n________________\n");
                            }
                        }
                    }
                    sbZohoDescription.Append("Product Name" + " : " + productName + "\n________________\n");

                    // attchment
                    if (dicAttachments.Count > 0)
                    {
                        foreach (var attchment in dicAttachments)
                        {
                            if (attchment.Value)
                                sbZohoDescription.Append(await _localizationService.GetResourceAsync("Customizationform.Fields.Attachment") + " : " +
                                    "<img src=\"" + attchment.Key + "\" width=\"100px\" height=\"100px\"/>" + "\n________________\n");
                            else
                                sbZohoDescription.Append(await _localizationService.GetResourceAsync("Customizationform.Fields.Attachment") + " : " +
                                   "<a href=\"" + attchment.Key + "\">" +
                                   await _localizationService.GetResourceAsync("Customizationform.Fields.Attachment.Link") + "</a>" + "\n________________\n");
                        }
                    }
                    // end
                    responseModel.Picture = picture;
                    responseModel.customProps = customProps;
                    responseModel.Total = product.Price;
                    await SendLeadToZoho(responseModel, sbZohoDescription);
                    string MailerHtml = await GetMailerHtml(responseModel, sbZohoDescription, product, picture);

                    await _workflowMessageService.SendCustomizationFormMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id,
                   model.Email.Trim(), model.FullName, " Hi " + model.FullName + ", Your Custom Requirements Have Been Received!", MailerHtml);

                    #region Mailchimp
                    var _mailchimpService = EngineContext.Current.Resolve<IMailchimpService>();
                    var _httpContextAccessor = EngineContext.Current.Resolve<IHttpContextAccessor>();
                    string url = _httpContextAccessor.HttpContext?.Request.Headers["Referer"];
                    string absoluteUrl = _httpContextAccessor.HttpContext?.Request.Headers["Referer"];
                    string userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"];
                    List<string> tags = new List<string>();
                    tags.Add("Leads");
                    await _mailchimpService.CartOperation(model.Email.Trim(), model.FullName, "", tags, url, userAgent, model.ProductId.ToString(), "");


                    #endregion
                    return Json(new
                    {
                        content = await RenderPartialViewToStringAsync("_CustomizationFormResponse", responseModel),
                        Success = true
                    });
                }
            }
            else
                return Json(new
                {
                    content = string.Join("; ", ModelState.Values
                                            .SelectMany(x => x.Errors)
                                            .Select(x => x.ErrorMessage)),
                    Success = false
                });


        }

        #endregion


        #region Utilities


        private async Task<string> GetMailerHtml(CustomizationFormDataModel responseModel, StringBuilder sbZohoDescription, Product product, string picture)
        {
            string sename = await _urlRecordService.GetSeNameAsync(product);
            StringBuilder html = new StringBuilder();

            html.Append("<table class=\"outer\" align=\"center\" style=\"border-spacing: 0;font-family: sans-serif;font-size:16px; color : #333;margin: 0 auto;width: 100%; max-width: 650px;border: 0.5px solid #ccc;box-shadow:0 0 10px 0 #ccc;\">");
            html.Append("<tr><td style=\"padding: 0;text-align:center;padding-top:2%;\"><div style=\"max-width: 400px;margin: auto;\"><a href=\"https://www.sierralivingconcepts.com?utm_source=Mailer&utm_medium=Email&utm_campaign=LeadAknwg\" target=\"_blank\" style=\"color: black;text-decoration: none;\"><img width=\"400px\" src=\"https://www.sierralivingconcepts.com/images/thumbs/0412917_logo.png\" style=\"border: 0;margin:auto;\" /></a></div></td></tr>");
            html.Append("<tr><td style=\"text-align:left;\"><p style=\"padding: 0 2% 0 4%;margin:10% 0 2% 0;font-weight:bold;\">");
            html.Append("Hello ");

            if (!string.IsNullOrEmpty(responseModel.FullName))
                html.Append(responseModel.FullName);

            html.Append("</p></td></tr>");
            html.Append("<tr><td style=\"text-align:left;\"><p style=\"padding: 0 2% 0 4%;margin:2% 0 2% 0;\">");
            html.Append("Greetings from Sierra Living Concepts!");
            html.Append("</p></td></tr>");
            html.Append("<tr><td style=\"text-align:left;\"><p style=\"padding: 0 2% 0 4%;margin:2% 0 0 0;\">");
            html.Append("Thank you for your interest in our product! We have received your requirements.");
            html.Append("</p><p style=\"padding: 0 2% 0 4%;margin:0 0 3% 0;\">Here's the copy of your acknowledgement form for your future reference.");
            html.Append("</p></td></tr>");
            html.Append("<tr><td style=\"text-align:left;\"><div style=\"padding: 0 2% 0 4%;margin:2% 0 2% 0;float: left;width: 94%;\">");
            html.Append("<h3 style=\"color:#444444;font-weight:bold;margin: 0 0 5px 0;font-size:18px;text-decoration:underline;\">Personal Details</h3>");
            html.Append("<table style=\"color:#333;font-size:16px;\">");

            if (!string.IsNullOrEmpty(responseModel.FullName))
                html.Append("<tr><td style=\"width:140px;font-weight:bold;\">Full Name:</td><td>" + responseModel.FullName + "</td></tr>");

            if (!string.IsNullOrEmpty(responseModel.Email))
                html.Append("<tr><td style=\"width:140px;font-weight:bold;\">Email:</td><td>" + responseModel.Email + "</td></tr>");

            if (!string.IsNullOrEmpty(responseModel.Phone))
                html.Append("<tr><td style=\"width:140px;font-weight:bold;\">Contact No:</td><td>" + responseModel.Phone + "</td></tr>");

            if (!string.IsNullOrEmpty(responseModel.ZipCode))
                html.Append("<tr><td style=\"width:140px;font-weight:bold;\">Zip:</td><td>" + responseModel.ZipCode + "</td></tr>");

            html.Append("</table></div></td></tr>");
            html.Append("<tr><td style=\"text-align:left;\"><div style=\"padding: 0 2% 0 4%;margin:2% 0 2% 0;float: left;width: 94%;\">");
            html.Append("<h3 style=\"color:#444444;font-weight:bold;margin: 0 0 5px 0;font-size:18px;text-decoration:underline;\">Product Details</h3>");
            html.Append("<table style=\"color:#333;font-size:16px;width:100%;float:left;\">");


            html.Append("<tr><td style=\"width:140px;font-weight:bold;\">Product ID:</td><td>" + responseModel.ProductId + "</td></tr>");

            if (!string.IsNullOrEmpty(responseModel.ProductName))
                html.Append("<tr><td style=\"width:140px;font-weight:bold;\">Product Name:</td><td>" + responseModel.ProductName + "</td></tr>");

            html.Append("</table>");
            html.Append("<table style=\"color:#333;font-size:16px;width:61%;float:left;\">");

            if (!string.IsNullOrEmpty(responseModel.SKU))
                html.Append("<tr><td style=\"width:140px;font-weight:bold;\">SKU:</td><td>" + responseModel.SKU + "</td></tr>");

            html.Append("<tr><td style=\"width:140px;font-weight:bold;\">Dimensions:</td><td></td></tr>");

            if (product.Length > 0)
                html.Append("<tr><td style=\"width:50px;padding-left:10.5%;font-weight:bold;\">Length:</td><td>" + product.Length + "</td></tr>");
            if (product.Width > 0) html.Append("<tr><td style=\"width:50px;padding-left:13%;font-weight:bold;\">Width:</td><td>" + product.Width + "</td></tr>");
            if (product.Height > 0) html.Append("<tr><td style=\"width:50px;padding-left:11.5%;font-weight:bold;\">Height:</td><td>" + product.Height + "</td></tr>");


            foreach (var prop in responseModel.customProps)
            {
                if (!prop.Key.ToLower().Contains("intereseditems") && !prop.Key.ToLower().Contains("picture"))
                    html.Append("<tr><td style=\"width:140px;font-weight:bold;\">" + prop.Key + ":</td><td>" + prop.Value + "</td></tr>");
            }
            if (!string.IsNullOrEmpty(responseModel.Message))
                html.Append("<tr><td style=\"width:140px;font-weight:bold;\">Additional Notes:</td><td>" + responseModel.Message + "</td></tr>");
            if (responseModel.Attachments.Count > 0)
            {
                foreach (var attachment in responseModel.Attachments)
                {
                    if (attachment.Value)
                        html.Append("<tr><td style=\"width:140px;font-weight:bold;\">" +
                            await _localizationService.GetResourceAsync("Customizationform.Fields.Attachment") + ":</td><td>" +
                            "<img src=\"" + attachment.Key + "\" width=\"100px\" height=\"100px\"/>" + "</td></tr>");

                    else
                        html.Append("<tr><td style=\"width:140px;font-weight:bold;\">" +
                          await _localizationService.GetResourceAsync("Customizationform.Fields.Attachment") + ":</td><td>" +
                     "<a href=\"" + attachment.Key + "\">" +
                           await _localizationService.GetResourceAsync("Customizationform.Fields.Attachment.Link") + "</a>" + "</td></tr>");

                }
            }

            foreach (var prop in responseModel.customProps)
            {
                if (prop.Key.ToLower().Contains("intereseditems"))
                    html.Append("<tr><td style=\"width:140px;font-weight:bold;\">Additional Notes:</td><td>" + Convert.ToString(prop.Value).Replace(System.Environment.NewLine, "<br />") + "</td></tr>");

            }

            html.Append("</table>");
            if (responseModel.ProductId > 0)
            {
                html.Append("<table style=\"color:#333;font-size:16px;width:39%;float:left;\">");
                html.Append("<tr><td style=\"text-align:right;\"><a href=\"https://www.sierralivingconcepts.com" + Url.RouteUrl("Product", new { id = product.Id, SeName = sename }) + "/?utm_source=Mailer&utm_medium=Email&utm_campaign=LeadAknwg\">" + (!string.IsNullOrEmpty(picture) ? "<img style=\"width:190px;height:170px;\" src=\"" + picture + "\" />" : "") + "</a></td></tr>");
                html.Append("</table>");
            }
            html.Append("</div></td></tr>");
            html.Append("<tr><td style=\"text-align:left;\"><p style=\"padding: 0 2% 0 4%;margin:3% 0 2% 0;\">One of our representatives will soon get in touch with you. However, if you wish to discuss anything on priority, you may contact us on 1-866-864-8488, or write an email to us at sales@sierralivingconcepts.com. We would be happy to assist you.</p></td></tr>");
            html.Append("<tr><td style=\"text-align:left;\"><p style=\"padding: 0 2% 0 4%;margin:3% 0 2% 0;\">Sierra Living Concepts</p></td></tr>");
            html.Append("<tr><td style=\"text-align:center;\"><p style=\"padding: 0 2% 0 4%;margin:3% 0 5% 0;\"><a href=\"https://www.sierralivingconcepts.com/?utm_source=Mailer&utm_medium=Email&utm_campaign=LeadAknwg\" style=\"text-decoration:none;\"><span style=\"color:#fff;background:#444444;padding:1.5% 3%;font-size:15px;cursor:pointer;\">Continue Shopping</span></a></p></td></tr>");
            html.Append("</table>");
            return html.ToString();
        }
        private async Task<int> SendLeadToZoho(CustomizationFormDataModel responseModel, StringBuilder sbZohoDescription)
        {
            int GALeadID = 0;
            try
            {
                var _zohoService = EngineContext.Current.Resolve<IZohoService>();
                ZohoDto oZoho = new ZohoDto();
                int min = 1;
                int max = 100000;
                Random random = new Random();
                GALeadID = random.Next(min, max);
                oZoho.GALeadID = GALeadID.ToString();
                oZoho.LeadSource = "NEW STORE - CUSTOMIZE AN ITEM";
                oZoho.FirstName = responseModel.FullName;
                oZoho.Email = responseModel.Email;
                oZoho.Phone = String.IsNullOrEmpty(responseModel.Phone) ? " - " : responseModel.Phone.ToString();
                oZoho.ZipCode = String.IsNullOrEmpty(responseModel.ZipCode) ? " - " : responseModel.ZipCode;
                oZoho.Message = responseModel.Message;
                oZoho.SKU = responseModel.SKU;
                oZoho.ProductName = responseModel.ProductName;
                oZoho.Description = Convert.ToString(sbZohoDescription);
                oZoho.IPAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                oZoho.Total = responseModel.Total;
                oZoho.customer = await _workContext.GetCurrentCustomerAsync();
                await _zohoService.SaveLead(oZoho);
                return GALeadID;
            }
            catch (Exception ex)
            {
                return GALeadID;



            }
        }

        #endregion
    }
}
