using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Customization.Custom
{
    public partial record CategorySuggestedKeywordModel : BaseNopEntityModel
    {
        public int CategoryId { get; set; }
        public string KeyWord { get; set; }
    }
}