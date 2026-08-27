using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Customization.Custom
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
