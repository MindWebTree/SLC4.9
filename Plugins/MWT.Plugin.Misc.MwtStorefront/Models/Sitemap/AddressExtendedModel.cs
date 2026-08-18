using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Common
{
  
    public partial record AddressExtendedModel : AddressModel
    {
        public string Abbreviation { get; set; }
        public string CountryTwoLetterSeoCode { get; set; }
        public string StateAbbreviation { get; set; }

        [NopResourceDisplayName("Address.Fields.FullName")]

        public string FullName { get; set; }
    }
}
