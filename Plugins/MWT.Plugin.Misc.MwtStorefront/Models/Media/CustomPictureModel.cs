using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Media
{
    public partial record CustomPictureModel : PictureModel
    {
        public bool IsDimensionImage { get; set; }
    }
}
