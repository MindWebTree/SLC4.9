using Nop.Web.Framework.Models;
using System.Collections.Generic;
using System.Text;


#nullable enable
namespace MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models
{
    public record MenuAdminModel : BaseNopEntityModel
    {

        public MenuAdminModel()
        {
        }
        public bool Enabled { get; set; }

        public string Name { get; set; }

        public string CssClass { get; set; }
          
    }
}
