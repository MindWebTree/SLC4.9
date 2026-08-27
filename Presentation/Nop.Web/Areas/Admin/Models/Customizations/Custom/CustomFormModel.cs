using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.Customization.Custom
{
    public partial record CustomFormModel : BaseNopEntityModel
    {
        #region Ctor

        public CustomFormModel()
        {

        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Catalog.CustomForm.Fields.Name")]
        public string FormName { get; set; }

        [NopResourceDisplayName("Admin.Catalog.CustomForm.Fields.Html")]
        public string FormHtml { get; set; }

        [NopResourceDisplayName("Admin.Catalog.CustomForm.Fields.ShowInPopup")]
        public bool ShowInPopup { get; set; }

        [NopResourceDisplayName("Admin.Catalog.CustomForm.Fields.ThankYouHtml")]
        public string ThankYouHtml { get; set; }

        [NopResourceDisplayName("Admin.Catalog.CustomForm.Fields.ThankYouPageLink")]
        public string ThankYouPageLink { get; set; }

        [NopResourceDisplayName("Admin.Catalog.CustomForm.Fields.SendCustomerNotification")]
        public bool SendCustomerNotification { get; set; }

        [NopResourceDisplayName("Admin.Catalog.CustomForm.Fields.BccEmailAddresses")]
        public string BccEmailAddresses { get; set; }

        [NopResourceDisplayName("Admin.Catalog.CustomForm.Fields.Subject")]
        public string Subject { get; set; }

        [NopResourceDisplayName("Admin.Catalog.CustomForm.Fields.Body")]
        public string Body { get; set; }


        [NopResourceDisplayName("Admin.Catalog.CustomForm.Fields.CustomerNotificationEmailBody")]
        public string CustomerNotificationEmailBody { get; set; }


        [NopResourceDisplayName("Admin.Catalog.CustomForm.Fields.CustomerNotificationSubject")]
        public string CustomerNotificationSubject { get; set; }

        [NopResourceDisplayName("Admin.Catalog.CustomForm.Fields.Published")]
        public bool Published { get; set; }

        public bool UseHtml { get; set; }

        [NopResourceDisplayName("Admin.Catalog.CustomForm.Fields.RenderActions")]
        public bool RenderActions { get; set; }

        #endregion
    }
}
