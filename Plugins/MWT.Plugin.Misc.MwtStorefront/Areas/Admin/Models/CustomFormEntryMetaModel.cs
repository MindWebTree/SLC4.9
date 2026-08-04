using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace MWT.Plugin.Misc.MwtStorefront.Areas.Admin.Models
{
    public partial record CustomFormEntryMetaModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Catalog.CustomFormEntryMeta.Fields.EntryID")]
        public int EntryID { get; set; }

        [NopResourceDisplayName("Admin.Catalog.CustomFormEntryMeta.Fields.MetaKey")]
        public string MetaKey { get; set; }

        [NopResourceDisplayName("Admin.Catalog.CustomFormEntryMeta.Fields.MetaValue")]
        public string MetaValue { get; set; }
    }
}
