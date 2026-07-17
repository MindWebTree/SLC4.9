using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.MegaMenu.Areas.Admin.Models
{
    public interface ICopyableEntityModel
    {
        CopyModel CopyModel { get; set; }
    }
}
