using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.Redirect.Models.Redirections
{
    public record RedirectionSearchModel : BaseSearchModel
    {
        public string Pattern { get; set; }
        public string RedirectUrl { get; set; }
        public ColumnOptions[] Columns { get; set; }
        public Order[] Order { get; set; }
    }

    public class ColumnOptions
    {
        public string Data { get; set; }
        public string Name { get; set; }

        public bool Searchable { get; set; }

        public bool Orderable { get; set; }

    }

    public class Order
    {
        public int Column { get; set; }
        public string Dir { get; set; }
    }

}
