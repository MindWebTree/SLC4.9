namespace MWT.Nop.Core.Services.Manage
{
    public partial interface IManageService
    {
        Task MarkWgsAsPaid(int orderNo, string email, decimal orderTotal, string pairedOrderIds);
        Task<(bool isValid, string message)> ValidateWgsOrder(int orderNo);
        Task<List<(int orderID, string serviceName)>> GetPairedOrders(int orderNo);
        Task<(string Comment, string ErrorMessage)> GetOrderFeedback(int orderId, string email);
        Task<(bool IsSaved, string ErrorMessage)> SaveOrderFeedbackRewardClaim(int orderId, string emailID, string comment, string giftProductName, bool isGoogleReviewed, bool isInstagramPost, bool isSharedVideo);
        Task SyncPendingOrderPartialPayment(int orderId, string transactionid, decimal payment, DateTime paidOn, string paymentgateway);
    }
}