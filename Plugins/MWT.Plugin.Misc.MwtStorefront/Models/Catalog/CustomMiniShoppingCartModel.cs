using Microsoft.AspNetCore.Mvc.Rendering;
using MWTNop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Models.ShoppingCart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Catalog
{
    public record CustomMiniShoppingCartModel : MiniShoppingCartModel
    {
        public new IList<CustomShoppingCartItemModel> Items { get; set; }
     = new List<CustomShoppingCartItemModel>();
        public string Heading { get; set; } 
        public List<int> cartItems { get; set; }


        public partial record CustomShoppingCartItemModel : ShoppingCartItemModel
        {
            public int ParentGroupedProductId { get; set; }
            public List<SelectListItem> AllowedQuantities => new List<SelectListItem>();
            public VariantCombination Variant { get; set; }
        }
    }
}
