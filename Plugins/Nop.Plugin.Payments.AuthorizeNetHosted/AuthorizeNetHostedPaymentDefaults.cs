namespace Nop.Plugin.Payments.AuthorizeNetHosted.Models
{
    public static class AuthorizeNetHostedPaymentDefaults
    {
        public static string DLLFileName => "Nop.Plugin.Payments.AuthorizeNetHosted.dll";
        public static string SolutionID => "AAA100302";   // Authorize.NET Solution ID
        public static string SolutionName => "AuthorizeNetHostedPayment";
        public static string WebhookHandlerRoute => "Plugin.Payments.AuthorizeNetHostedPayment.WebhookHandler";
        public static string WebhookUrl => "AuthorizeNetHosted/WebhookHandler";
        public static string TransactionValue => "TransactionId";
        public static string OrderId => "OrderIdRef";
        public static string AuthorizeNetHostedPaymentDataValue => "AuthorizeNetHostedPaymentDataValue";
        public static string AuthorizeNetHostedPaymentDataDescriptor => "AuthorizeNetHostedPaymentDataDescriptor";

        public static string SandboxFormUrl = "https://test.authorize.net/payment/payment";

        public static string ProductionFormUrl = "https://accept.authorize.net/payment/payment";
    }
}