using Nop.Core;
using Nop.Core.Domain.Stores;

namespace MWT.Nop.Plugin.MegaMenu.Domain
{
    public class Menu : BaseEntity, IStoreMappingSupported
    {
        public bool Enabled { get; set; }

        public string Name { get; set; }

        public string CssClass { get; set; }

        public bool ShowDropdownsOnClick { get; set; }

        public bool LimitedToStores { get; set; }
    }
}
