using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.FAQModule
{
    public partial record FaqModel
    {
        public string Question { get; set; }
        public string Answer { get; set; }
    }
}
