using Nop.Web.Framework.Models;
using System.Collections.Generic;
using System.Text;


#nullable enable
namespace MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models
{
    public record EntityGridModel : BaseNopEntityModel
    {
        public EntityGridModel()
        {
        }

        public string Name { get; set; }
      
    }
}
