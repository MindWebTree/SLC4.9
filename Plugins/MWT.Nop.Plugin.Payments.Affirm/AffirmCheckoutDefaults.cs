using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Payments.Affirm
{
    public class AffirmCheckoutDefaults
    {
        public static string ConfirmCallbackHandlerRoute => "Plugin.Payments.Affirm.ConfirmCallbackHandler";
        public static string CancelCallbackHandlerRoute => "Plugin.Payments.Affirm.CancelCallbackHandler";

        public static string CustomConfirmCallbackHandlerRoute => "Plugin.Payments.Affirm.CustomOrderConfirmCallbackHandler";
        public static string CustomCancelCallbackHandlerRoute => "Plugin.Payments.Affirm.CustomOrderCancelCallbackHandler";
        public static string ConfigurationRouteName => "Plugin.Payments.Affirm.Configure";

        public static string PAYMENT_INFO_VIEW_COMPONENT_NAME = "AffirmPaymentInfo";

        public static string WIDGET_COMPONENT_NAME = "AffirmPaymentWidget";

        public static string WIDGET_PROMOTION_MESSAGE_COMPONENT_NAME = "AffirmPromotionMessageWidget";
        public static string SystemName => "Payments.Affirm";
    }
}
