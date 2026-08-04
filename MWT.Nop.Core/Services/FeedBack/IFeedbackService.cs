namespace MWT.Nop.Core.Services.FeedBack
{
    public partial interface IFeedbackService
    {
       Task<string> GetProductFeedBacks(int pageSize,string sku,int mainCategoryId);
    }
}
