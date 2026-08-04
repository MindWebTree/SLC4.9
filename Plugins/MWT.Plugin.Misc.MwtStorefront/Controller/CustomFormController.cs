using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MWT.Nop.Core.Domain;
using MWT.Nop.Core.Domain.Zoho;
using MWT.Nop.Core.Infrastructure;
using MWT.Nop.Core.Service.Zoho;
using MWT.Nop.Core.Services.Custom;
using MWT.Nop.Core.Services.MailChimp;
using MWT.Nop.Core.Services.Message;
using MWT.Nop.Core.Services.Shared;
using MWT.Plugin.Misc.MwtStorefront.Models.Custom;
using Nop.Core;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Security;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System.Text;

namespace MWT.Plugin.Misc.MwtStorefront.Controllers
{
    public partial class CustomFormController : BasePublicController
    {
        #region fields 

        private readonly ICustomFormService _customFormService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly CaptchaSettings _captchaSettings;
        private readonly INopFileProvider _nopFileProvider;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ICustomWorkflowMessageService _workflowMessageService;
        private readonly IMailchimpService _mailchimpService;
        private readonly ILogger _logger;
        private readonly IZohoService _zohoService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICommonService _commonService;
        #endregion
        public CustomFormController(ICustomFormService customFormService, ILocalizationService localizationService, ISettingService settingService,
            CaptchaSettings captchaSettings, INopFileProvider nopFileProvider, IStoreContext storeContext, IWorkContext workContext,
            ICustomWorkflowMessageService workflowMessageService, IMailchimpService mailchimpService, ILogger logger, IZohoService zohoServic,
            IHttpContextAccessor httpContextAccessor, ICommonService commonService)
        {
            _customFormService = customFormService;
            _localizationService = localizationService;
            _settingService = settingService;
            _captchaSettings = captchaSettings;
            _nopFileProvider = nopFileProvider;
            _storeContext = storeContext;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _mailchimpService = mailchimpService;
            _logger = logger;
            _zohoService = zohoServic;
            _httpContextAccessor = httpContextAccessor;
            _commonService = commonService;
        }


        [CheckAccessPublicStore(true)]
        [HttpPost]
        [ValidateCaptcha]
        public async Task<IActionResult> SubmitCustomInquiry(IFormCollection form, bool captchaValid)
        {

            string[] skippFormKeys = (await _localizationService.GetResourceAsync("Customform.Skip.FormKeys")).Split(',');
            CustomFormModelResponse model = new CustomFormModelResponse();
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage && !captchaValid)
                model.ErrorMessage = await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage");

            else
            {
                var formId = 0;
                try
                {


                    foreach (var formKey in form.Keys)
                        if (formKey.Equals("customform_id", StringComparison.InvariantCultureIgnoreCase))
                        {
                            int.TryParse(form[formKey], out formId);
                            break;
                        }

                    if (formId == 0)
                        model.ErrorMessage = await _localizationService.GetResourceAsync("Customform.NotFound");


                    else
                    {



                        var customform = await _customFormService.GetCustomformById(formId);
                        if (customform == null)
                            model.ErrorMessage = await _localizationService.GetResourceAsync("Customform.NotFound");


                        else
                        {
                            int uploadMaxSize = await _settingService.GetSettingByKeyAsync<int>("Customizationform.UploadMaxSize");
                            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                            bool newsLetterSubscribed = false;
                            // validate google Captcha
                            Dictionary<string, string> formData = new Dictionary<string, string>();
                            List<Token> tokens = new List<Token>();
                            foreach (var formKey in form.Keys)
                            {
                                if (!string.IsNullOrEmpty(form[formKey]) && formKey != "__RequestVerificationToken"
                                    && formKey != "g-recaptcha-response")
                                {
                                    if (formKey == "newsletter_subscribed")
                                        newsLetterSubscribed = true;
                                    // skip google captcha and other keys of form ids
                                    formData.Add(formKey.Trim(), form[formKey]);
                                    tokens.Add(new Token(
                                        formKey.Trim(),
                                      form[formKey]
                                    ));
                                }
                            }
                            string customerEmailAddress = "";
                            customerEmailAddress = formData.Where(m => m.Key.Trim().Equals("email", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault().Value;

                            string fakeCustomerEmails = await _settingService.GetSettingByKeyAsync<string>("Common.FakeCustomer.Emails");
                            if (CustomCommonHelper.IsFakeCustomer(fakeCustomerEmails, customerEmailAddress))
                            {
                                model.ErrorMessage = await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage");
                                return new JsonResult(model);
                            }
                            #region Insert Entries
                            var customformEntry = new CustomFormEntry()
                            {
                                CreatedOnUtc = DateTime.UtcNow,
                                FormId = formId,
                                UpdatedOnUtc = DateTime.UtcNow
                            };
                            await _customFormService.InsertCustomFormsEntryAsync(customformEntry);

                            #region Files

                            foreach (var file in form.Files)
                            {
                                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                                {
                                    model.ErrorMessage = $"File '{file.FileName}' is not a valid image type.";
                                    return new JsonResult(model);

                                }
                                if (file.Length > uploadMaxSize)
                                {
                                    model.ErrorMessage = string.Format(
                                        await _localizationService.GetResourceAsync("Customizationform.Fields.Attachment.MaxUploadSize")
                                        , uploadMaxSize);
                                    return new JsonResult(model);
                                }

                            }
                            Dictionary<string, bool> dicAttachments = new Dictionary<string, bool>();
                            foreach (var file in form.Files)
                            {
                                string fileName = Guid.NewGuid().ToString() + "-" + file.FileName;

                                string filePath = Path.Combine(_nopFileProvider.MapPath("/wwwroot/images/customform/"),
                                  fileName);
                                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(fileStream);
                                    dicAttachments.Add(_storeContext.GetCurrentStore().Url + "images/customform/" + fileName, (await _settingService.GetSettingByKeyAsync<string>("Customizationform.Supported.UploadTypes.Image")).ToLower().Split(',').Contains(Path.GetExtension(file.FileName).ToLower()));
                                }
                            }
                            if (dicAttachments.Any())
                            {
                                await _customFormService.InsertCustomFormEntryMetaAsync(new CustomFormEntryMeta()
                                {
                                    EntryID = customformEntry.Id,
                                    MetaKey = "Files",
                                    MetaValue = string.Join(',', dicAttachments.Keys)
                                }); ;

                                tokens.Add(new Token("Files", string.Join("", dicAttachments.Keys.Select(k => $"<div><img src='{k}' width='100' height='100' /></div>")), true));
                            }

                            #endregion

                            foreach (var formEntry in formData)
                            {
                                if (!skippFormKeys.Where(m => m.Trim().Equals(formEntry.Key, StringComparison.InvariantCultureIgnoreCase)).Any())
                                    await _customFormService.InsertCustomFormEntryMetaAsync(new CustomFormEntryMeta()
                                    {
                                        EntryID = customformEntry.Id,
                                        MetaKey = formEntry.Key,
                                        MetaValue = formEntry.Value
                                    });
                            }

                            #endregion


                            #region Email

                            string subject = "";
                            string emailContent = "";

                            if (!string.IsNullOrEmpty(customform.Body))
                            {
                                emailContent =await this._commonService.ReplaceTokens(customform.Body, tokens);
                                subject = await this._commonService.ReplaceTokens(customform.Subject, tokens);
                                await _workflowMessageService.SendCustomFormMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id, "", "",
                                       String.IsNullOrEmpty(customform.BccEmailAddresses) ? "" : customform.BccEmailAddresses, subject, emailContent);
                            }

                            string firstName = "";
                            string lastName = "";
                            string name = "";


                            firstName = formData.Where(m => m.Key.Trim().Equals("firstName", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault().Value;
                            lastName = formData.Where(m => m.Key.Trim().Equals("lastName", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault().Value;
                            name = formData.Where(m => m.Key.Trim().Equals("name", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault().Value;

                            if (string.IsNullOrEmpty(name))
                                name = (string.IsNullOrEmpty(firstName) ? "" : firstName + " ") + (string.IsNullOrEmpty(lastName) ? "" : lastName);

                            if (customform.SendCustomerNotification && !string.IsNullOrEmpty(customform.CustomerNotificationEmailBody))
                            {
                                if (!string.IsNullOrEmpty(customerEmailAddress))
                                {

                                    emailContent = await this._commonService.ReplaceTokens(customform.CustomerNotificationEmailBody, tokens);
                                    subject = await this._commonService.ReplaceTokens(customform.CustomerNotificationSubject, tokens);
                                    await _workflowMessageService.SendCustomFormMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id, customerEmailAddress, name,
                                           "", subject, emailContent);
                                }
                            }

                            if (newsLetterSubscribed)
                            {
                                if (string.IsNullOrEmpty(customerEmailAddress))
                                {

                                    await _logger.InsertLogAsync(LogLevel.Error, "Failed to add user Under NewsLetter  for  form  " + formId,
                                        "Logic failed to find Email Address of Customer");
                                }
                                else
                                {



                                    string url = _httpContextAccessor.HttpContext?.Request.Headers["Referer"];
                                    string absoluteUrl = _httpContextAccessor.HttpContext?.Request.Headers["Referer"];
                                    string userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"];
                                    List<string> tags = new List<string>();
                                    tags.Add("Leads");
                                    await _mailchimpService.CartOperation(customerEmailAddress, name, customform.FormName, tags, url, userAgent, "", "");


                                    //await _iNewsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new Core.Domain.Messages.NewsLetterSubscription()
                                    //{
                                    //    Email = customerEmailAddress,
                                    //    Active = true,
                                    //    CreatedOnUtc = DateTime.UtcNow,
                                    //    StoreId = _storeContext.GetCurrentStore().Id,
                                    //    UpdatedOnUtc = DateTime.UtcNow,
                                    //    NewsLetterSubscriptionGuid = Guid.NewGuid()

                                    //});
                                }


                            }

                            #endregion

                            #region Zoho Lead


                            try
                            {
                                StringBuilder sbZohoDescription = new StringBuilder();
                                ZohoDto oZoho = new ZohoDto();
                                oZoho.FirstName = oZoho.FullName = name;
                                oZoho.Email = customerEmailAddress;
                                foreach (var formKey in form.Keys)
                                {
                                    if (!string.IsNullOrEmpty(form[formKey]) && formKey != "__RequestVerificationToken"
                                        && formKey != "g-recaptcha-response")
                                    {
                                        if (formKey != "newsletter_subscribed")
                                        {
                                            sbZohoDescription.Append(formKey.Trim() + " : " + form[formKey] + "\n________________\n");

                                            if (string.Equals(formKey.Trim(), "phone", StringComparison.InvariantCultureIgnoreCase))
                                                oZoho.Phone = form[formKey];
                                            else if (string.Equals(formKey.Trim(), "message", StringComparison.InvariantCultureIgnoreCase))
                                                oZoho.Message = form[formKey];
                                        }
                                    }
                                }

                                int min = 1;
                                int max = 100000;
                                Random random = new Random();
                                oZoho.GALeadID = random.Next(min, max).ToString();
                                oZoho.LeadSource = $"NEW STORE - {customform.FormName}";
                                oZoho.Phone = String.IsNullOrEmpty(oZoho.Phone) ? " - " : oZoho.Phone.ToString();
                                oZoho.ZipCode = " - ";
                                oZoho.Description = Convert.ToString(sbZohoDescription);
                                oZoho.IPAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                                var glclidCookie = Request.Cookies["gclid"];
                                if (glclidCookie != null)
                                    oZoho.GCLID = glclidCookie;
                                await _zohoService.SaveLead(oZoho);
                            }
                            catch
                            {

                            }
                            #endregion


                            #region Response
                            model.ThankYouPageLink = customform.ThankYouPageLink;
                            model.ThankYouHtml = await this._commonService.ReplaceTokens(customform.ThankYouHtml, tokens);
                            model.ErrorMessage = "";
                            model.ShowInPopup = customform.ShowInPopup;


                            #endregion


                        }
                    }
                }
                catch (Exception exp)
                {

                    await _logger.InsertLogAsync(LogLevel.Error, "Error Happened in Custom form Submission, Form Id " + formId,
                        exp.Message);
                    model.ErrorMessage = await _localizationService.GetResourceAsync("Customform.Common.ExceptionMessage");
                }
            }
            return new JsonResult(model);
        }
    }
}