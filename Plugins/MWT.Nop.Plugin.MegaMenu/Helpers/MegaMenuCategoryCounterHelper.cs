
using System.Collections.Generic;

namespace MWT.Nop.Plugin.MegaMenu.Helpers
{
    public class MegaMenuCategoryCounterHelper : IMegaMenuCategoryCounterHelper
    {
        private IDictionary<int, int> _megaMenuCategoryCountCollection;

        public MegaMenuCategoryCounterHelper() => this._megaMenuCategoryCountCollection = (IDictionary<int, int>)new Dictionary<int, int>();

        public int GetCategoryCount(int menuId)
        {
            int num;
            if (this._megaMenuCategoryCountCollection.TryGetValue(menuId, out num))
                return this._megaMenuCategoryCountCollection[menuId] = num + 1;
            this._megaMenuCategoryCountCollection.Add(menuId, 1);
            return 1;
        }
    }
}
