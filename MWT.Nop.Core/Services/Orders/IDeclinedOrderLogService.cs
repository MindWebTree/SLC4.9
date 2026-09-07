namespace MWT.Nop.Core.Services.Orders
{
    public partial interface IDeclinedOrderLogService
    {
        Task Insert(string error);
    }
}
