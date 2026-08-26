using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Customization.Custom
{
    public partial record ProductSuggestedKeywordModel : BaseNopEntityModel
    {
        public int ProductId { get; set; }
        public int SuggestedKeyWordId { get; set; }
        public bool IsCustom { get; set; }
        public string KeyWord { get; set; }
    }
}