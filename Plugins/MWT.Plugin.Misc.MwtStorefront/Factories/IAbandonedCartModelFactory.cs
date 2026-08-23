using MWT.Plugin.Misc.MwtStorefront.Models.AbandonedCart;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public partial interface IAbandonedCartModelFactory
    {
        Task<AbandonedCartModel> PrepareAbandonedCartModel(string invoiceId);
        Task<List<AbandonedCartModel>> AbandonedCardList();
    }
}
