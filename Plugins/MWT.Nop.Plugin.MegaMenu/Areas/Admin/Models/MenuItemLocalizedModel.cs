using Nop.Web.Framework.Models;
using System;

namespace MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models
{
    public class MenuItemLocalizedModel : ILocalizedLocaleModel, ICloneable
    {
        public int LanguageId { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public object Clone() => (object)new MenuItemLocalizedModel()
        {
            LanguageId = this.LanguageId,
            Title = this.Title,
            Url = this.Url
        };
    }
}
